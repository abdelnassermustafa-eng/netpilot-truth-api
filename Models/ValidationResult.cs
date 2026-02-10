namespace TruthApi.Models;

public class ValidationResult
{
    public string Rule { get; set; } = "";
    public string ResourceId { get; set; } = "";
    public string Status { get; set; } = "";   // PASS or FAIL
    public string Severity { get; set; } = ""; // INFO, WARNING, CRITICAL
    public ValidationSeverity SeverityLevel { get; set; } = ValidationSeverity.Info;
    public string Message { get; set; } = "";
}
