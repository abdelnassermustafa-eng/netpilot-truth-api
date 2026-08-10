using TruthApi.Models.Platform.Operations;

namespace TruthApi.Services.Platform.Operations;

public interface IOperationPlanner
{
    bool CanHandle(OperationRequest request);

    Task<ExecutionPlan> CreatePlanAsync(
        OperationRequest request,
        OperationValidationResult validation,
        CancellationToken cancellationToken = default);
}
