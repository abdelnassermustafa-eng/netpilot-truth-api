using TruthApi.Models.Platform.Operations;

namespace TruthApi.Services.Platform.Operations;

public interface IOperationValidator
{
    bool CanHandle(OperationRequest request);

    Task<OperationValidationResult> ValidateAsync(
        OperationRequest request,
        CancellationToken cancellationToken = default);
}
