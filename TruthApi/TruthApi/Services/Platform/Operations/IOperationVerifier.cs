using TruthApi.Models.Platform.Operations;

namespace TruthApi.Services.Platform.Operations;

public interface IOperationVerifier
{
    bool CanHandle(OperationRequest request);

    Task<OperationVerificationResult> VerifyAsync(
        OperationRequest request,
        ExecutionPlan plan,
        CancellationToken cancellationToken = default);
}
