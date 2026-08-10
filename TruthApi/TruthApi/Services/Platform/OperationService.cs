using TruthApi.Models.Platform.Operations;
using TruthApi.Services.Platform.Operations;
using PlatformOperation =
    TruthApi.Models.Platform.Operations.Operation;

namespace TruthApi.Services.Platform;

/// <summary>
/// Moves an infrastructure Operation through validation, planning,
/// execution, and Discovery-based verification.
///
/// Persistence remains the responsibility of OperationManager.
/// </summary>
public sealed class OperationService
{
    private readonly IReadOnlyList<IOperationValidator> _validators;
    private readonly IReadOnlyList<IOperationPlanner> _planners;
    private readonly IReadOnlyList<IOperationExecutor> _executors;
    private readonly IReadOnlyList<IOperationVerifier> _verifiers;

    public OperationService(
        IEnumerable<IOperationValidator> validators,
        IEnumerable<IOperationPlanner> planners,
        IEnumerable<IOperationExecutor> executors,
        IEnumerable<IOperationVerifier> verifiers)
    {
        _validators = validators.ToList();
        _planners = planners.ToList();
        _executors = executors.ToList();
        _verifiers = verifiers.ToList();
    }

    public async Task<PlatformOperation> ExecuteAsync(
        PlatformOperation operation,
        OperationCheckpoint checkpoint,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(operation);
        ArgumentNullException.ThrowIfNull(checkpoint);

        if (operation.IsTerminal)
        {
            throw new InvalidOperationException(
                $"Operation '{operation.OperationId}' is already " +
                $"complete with status {operation.Status}.");
        }

        try
        {
            var request = ToRequest(operation);

            var validator = SelectSingle(
                _validators,
                candidate => candidate.CanHandle(request),
                "validator",
                request);

            var expectedVersion = operation.Version;
            operation.BeginValidation();

            await checkpoint(
                operation,
                expectedVersion,
                cancellationToken);

            var validation = await validator.ValidateAsync(
                request,
                cancellationToken);

            expectedVersion = operation.Version;
            operation.ApplyValidation(validation);

            await checkpoint(
                operation,
                expectedVersion,
                cancellationToken);

            if (!validation.IsValid)
            {
                return operation;
            }

            var planner = SelectSingle(
                _planners,
                candidate => candidate.CanHandle(request),
                "planner",
                request);

            expectedVersion = operation.Version;
            operation.BeginPlanning();

            await checkpoint(
                operation,
                expectedVersion,
                cancellationToken);

            var plan = await planner.CreatePlanAsync(
                request,
                validation,
                cancellationToken);

            expectedVersion = operation.Version;
            operation.ApplyPlan(plan);

            await checkpoint(
                operation,
                expectedVersion,
                cancellationToken);

            if (operation.Status ==
                OperationStatus.AwaitingConfirmation)
            {
                return operation;
            }

            var executor = SelectSingle(
                _executors,
                candidate => candidate.CanHandle(plan),
                "executor",
                request);

            expectedVersion = operation.Version;
            operation.BeginExecution();

            await checkpoint(
                operation,
                expectedVersion,
                cancellationToken);

            await executor.ExecuteAsync(
                plan,
                cancellationToken);

            var verifier = SelectSingle(
                _verifiers,
                candidate => candidate.CanHandle(request),
                "verifier",
                request);

            /*
             * The provider verifier will eventually rediscover the
             * resource and return both observed state and comparison.
             *
             * Until that contract is expanded, preserve an explicit
             * verification snapshot showing that Discovery verification
             * has begun.
             */
            var observedState = new OperationStateSnapshot
            {
                Json = "{}",
                Source = "DiscoveryVerification"
            };

            expectedVersion = operation.Version;
            operation.BeginVerification(observedState);

            await checkpoint(
                operation,
                expectedVersion,
                cancellationToken);

            var verification = await verifier.VerifyAsync(
                request,
                plan,
                cancellationToken);

            expectedVersion = operation.Version;
            operation.ApplyVerification(verification);

            await checkpoint(
                operation,
                expectedVersion,
                cancellationToken);

            return operation;
        }
        catch (OperationCanceledException)
            when (cancellationToken.IsCancellationRequested)
        {
            if (!operation.IsTerminal)
            {
                var expectedVersion = operation.Version;

                operation.Cancel(
                    "The infrastructure operation was cancelled.");

                await checkpoint(
                    operation,
                    expectedVersion,
                    CancellationToken.None);
            }

            return operation;
        }
        catch (Exception exception)
        {
            if (!operation.IsTerminal)
            {
                var expectedVersion = operation.Version;

                operation.Fail(exception.Message);

                await checkpoint(
                    operation,
                    expectedVersion,
                    CancellationToken.None);
            }

            return operation;
        }
    }

    private static OperationRequest ToRequest(
        PlatformOperation operation)
    {
        return new OperationRequest
        {
            OperationId = operation.OperationId,
            Provider = operation.Provider,
            Service = operation.Service,
            ResourceType = operation.ResourceType,
            ResourceId = operation.ResourceId,
            Operation = operation.OperationName,
            Region = operation.Region,
            Parameters = operation.Parameters,
            RequestedBy = operation.RequestedBy,
            RequestedAt = operation.RequestedAt
        };
    }

    private static T SelectSingle<T>(
        IReadOnlyCollection<T> candidates,
        Func<T, bool> predicate,
        string componentName,
        OperationRequest request)
    {
        var matches = candidates
            .Where(predicate)
            .Take(2)
            .ToList();

        return matches.Count switch
        {
            1 => matches[0],

            0 => throw new InvalidOperationException(
                $"No operation {componentName} supports " +
                $"{request.Provider}/{request.Service}/" +
                $"{request.ResourceType}/{request.Operation}."),

            _ => throw new InvalidOperationException(
                $"Multiple operation {componentName}s support " +
                $"{request.Provider}/{request.Service}/" +
                $"{request.ResourceType}/{request.Operation}.")
        };
    }
}
