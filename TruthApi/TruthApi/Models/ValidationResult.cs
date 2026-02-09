namespace TruthApi.Models;

public class ValidationResult
{
    public string Rule { get; set; } = "";
    public string ResourceId { get; set; } = "";
    public string Status { get; set; } = "";   // PASS or FAIL
    public string Severity { get; set; } = ""; // INFO, WARNING, CRITICAL
    public string Message { get; set; } = "";

    // Phase 6.1 — Enriched fields
    public string Category { get; set; } = "";
    public string ResourceType { get; set; } = "";
    public string CidrOrDestination { get; set; } = "";
    public string RouteTableId { get; set; } = "";
    public string Region { get; set; } = "";
    public string Action { get; set; } = "";
}