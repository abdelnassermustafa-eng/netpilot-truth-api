using TruthApi.Models.Platform.Operations;

namespace TruthApi.Services.Platform.Operations;

public interface IOperationExecutor
{
    bool CanHandle(ExecutionPlan plan);

    Task ExecuteAsync(
        ExecutionPlan plan,
        CancellationToken cancellationToken = default);
}
