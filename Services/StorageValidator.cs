using TruthApi.Models;

namespace TruthApi.Services;

public class StorageValidator
{
    public List<ValidationResult> Run()
    {
        return new List<ValidationResult>
        {
            new ValidationResult
            {
                Rule = "storage.bucket.versioning",
                ResourceId = "demo-bucket",
                Status = "WARNING",
                Severity = "WARNING",
                SeverityLevel = ValidationSeverity.Warning,
                Message = "Bucket versioning is not enabled"
            }
        };
    }
}
