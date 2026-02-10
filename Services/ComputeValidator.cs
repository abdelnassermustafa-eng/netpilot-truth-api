using TruthApi.Models;

namespace TruthApi.Services;

public class ComputeValidator
{
    public List<ValidationResult> Run()
    {
        return new List<ValidationResult>
        {
            new ValidationResult
            {
                Rule = "compute.instance.exists",
                ResourceId = "i-demo-instance",
                Status = "PASS",
                Severity = "INFO",
                SeverityLevel = ValidationSeverity.Info,
                Message = "Demo compute instance check passed"
            }
        };
    }
}
