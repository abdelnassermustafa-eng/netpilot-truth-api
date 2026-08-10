using System.Text.Json;
using System.Text.Json.Serialization;
using TruthApi.Models.Platform.Operations;
using PlatformOperation =
    TruthApi.Models.Platform.Operations.Operation;

namespace TruthApi.Services.Platform.Operations;

/// <summary>
/// Durable, file-backed Operation Store.
///
/// Operations are stored as individual JSON documents so operation
/// history survives application restarts.
/// </summary>
public sealed class JsonFileOperationStore : IOperationStore
{
    private readonly string _directory;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly SemaphoreSlim _gate = new(1, 1);

    public JsonFileOperationStore(
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        var configuredPath = configuration[
            "Platform:Operations:StoragePath"];

        _directory = string.IsNullOrWhiteSpace(configuredPath)
            ? Path.Combine(
                environment.ContentRootPath,
                "App_Data",
                "operations")
            : Path.GetFullPath(
                configuredPath,
                environment.ContentRootPath);

        Directory.CreateDirectory(_directory);

        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy =
                JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition =
                JsonIgnoreCondition.WhenWritingNull,
            Converters =
            {
                new JsonStringEnumConverter()
            }
        };
    }

    public async Task CreateAsync(
        PlatformOperation operation,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(operation);

        await _gate.WaitAsync(cancellationToken);

        try
        {
            var path = GetPath(operation.OperationId);

            if (File.Exists(path))
            {
                throw new InvalidOperationException(
                    $"Operation '{operation.OperationId}' " +
                    "already exists.");
            }

            await WriteDocumentAsync(
                path,
                operation.ToDocument(),
                cancellationToken);
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task<PlatformOperation?> GetAsync(
        string operationId,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(operationId);

        await _gate.WaitAsync(cancellationToken);

        try
        {
            var document = await ReadDocumentAsync(
                GetPath(operationId),
                cancellationToken);

            return document is null
                ? null
                : PlatformOperation.Rehydrate(document);
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task<IReadOnlyList<PlatformOperation>> ListAsync(
        OperationStatus? status = null,
        int skip = 0,
        int take = 100,
        CancellationToken cancellationToken = default)
    {
        if (skip < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(skip));
        }

        if (take is < 1 or > 1000)
        {
            throw new ArgumentOutOfRangeException(
                nameof(take),
                "Take must be between 1 and 1000.");
        }

        await _gate.WaitAsync(cancellationToken);

        try
        {
            var operations = new List<PlatformOperation>();

            foreach (var path in Directory.EnumerateFiles(
                         _directory,
                         "*.json",
                         SearchOption.TopDirectoryOnly))
            {
                cancellationToken.ThrowIfCancellationRequested();

                var document = await ReadDocumentAsync(
                    path,
                    cancellationToken);

                if (document is null)
                {
                    continue;
                }

                if (status.HasValue &&
                    document.Status != status.Value)
                {
                    continue;
                }

                operations.Add(
                    PlatformOperation.Rehydrate(document));
            }

            return operations
                .OrderByDescending(item => item.CreatedAt)
                .ThenBy(item => item.OperationId)
                .Skip(skip)
                .Take(take)
                .ToList();
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task SaveAsync(
        PlatformOperation operation,
        long expectedVersion,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(operation);

        await _gate.WaitAsync(cancellationToken);

        try
        {
            var path = GetPath(operation.OperationId);

            var existing = await ReadDocumentAsync(
                path,
                cancellationToken);

            if (existing is null)
            {
                throw new OperationNotFoundException(
                    operation.OperationId);
            }

            if (existing.Version != expectedVersion)
            {
                throw new OperationConcurrencyException(
                    operation.OperationId,
                    expectedVersion,
                    existing.Version);
            }

            await WriteDocumentAsync(
                path,
                operation.ToDocument(),
                cancellationToken);
        }
        finally
        {
            _gate.Release();
        }
    }

    private string GetPath(string operationId)
    {
        var safeId = new string(
            operationId
                .Where(character =>
                    char.IsLetterOrDigit(character) ||
                    character is '-' or '_')
                .ToArray());

        if (!string.Equals(
                safeId,
                operationId,
                StringComparison.Ordinal))
        {
            throw new ArgumentException(
                "Operation ID contains unsupported characters.",
                nameof(operationId));
        }

        return Path.Combine(
            _directory,
            $"{safeId}.json");
    }

    private async Task<OperationDocument?> ReadDocumentAsync(
        string path,
        CancellationToken cancellationToken)
    {
        if (!File.Exists(path))
        {
            return null;
        }

        await using var stream = new FileStream(
            path,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            bufferSize: 4096,
            useAsync: true);

        return await JsonSerializer.DeserializeAsync<
            OperationDocument>(
                stream,
                _jsonOptions,
                cancellationToken);
    }

    private async Task WriteDocumentAsync(
        string path,
        OperationDocument document,
        CancellationToken cancellationToken)
    {
        var temporaryPath =
            $"{path}.{Guid.NewGuid():N}.tmp";

        try
        {
            await using (var stream = new FileStream(
                temporaryPath,
                FileMode.CreateNew,
                FileAccess.Write,
                FileShare.None,
                bufferSize: 4096,
                useAsync: true))
            {
                await JsonSerializer.SerializeAsync(
                    stream,
                    document,
                    _jsonOptions,
                    cancellationToken);

                await stream.FlushAsync(cancellationToken);
            }

            File.Move(
                temporaryPath,
                path,
                overwrite: true);
        }
        finally
        {
            if (File.Exists(temporaryPath))
            {
                File.Delete(temporaryPath);
            }
        }
    }
}
