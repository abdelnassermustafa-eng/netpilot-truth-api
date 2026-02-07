namespace TruthApi.Models;

public class ValidationReport
{
    public ValidationSummary Summary { get; set; } = new();
    public List<ValidationResult> Results { get; set; } = new();
}

