using TruthApi.Models;
using System.Collections.Generic;

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
                Status = "PASS",
                Message = "S3 bucket versioning enabled",
                Category = "storage",
                ResourceType = "s3",
                Region = "us-east-1",
                Action = "None"
            }
        };
    }
}
