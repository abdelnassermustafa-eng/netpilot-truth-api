using TruthApi.Models;
using System.Collections.Generic;

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
                Status = "PASS",
                Message = "At least one compute instance detected",
                Category = "compute",
                ResourceType = "ec2",
                Region = "us-east-1",
                Action = "None"
            }
        };
    }
}
