using TruthApi.Models;

namespace TruthApi.Services;

return new List<ValidationResult>
    {
        new ValidationResult
        {
    Rule = "compute.instance.exists",
            Status = "PASS",
            Severity = "INFO",
            Message = "At least one compute instance detected",
            Category = "compute",
            ResourceType = "ec2",
            ResourceId = "global",
            Region = "us-east-1",
            CidrOrDestination = "N/A",
            RouteTableId = "N/A",
            Action = "None"
        }
    };

}ß
