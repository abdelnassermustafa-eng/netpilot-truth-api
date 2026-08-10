using TruthApi.Models.Platform.Operations;

namespace TruthApi.Services.Platform.Operations;

public interface IOperationAuditStore
{
    Task SaveAsync(
        OperationAuditRecord record,
        CancellationToken cancellationToken = default);
}
