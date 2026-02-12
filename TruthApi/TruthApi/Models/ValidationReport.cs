namespace TruthApi.Models;

public class ValidationReport
{
    public ValidationSummary Summary { get; set; } = new();
    public List<ValidationResult> Results { get; set; } = new();

    // Phase 6.2 — resource inventory
    public List<VpcResourceRow> Vpcs { get; set; } = new();
    public List<SubnetResourceRow> Subnets { get; set; } = new();
    public List<RouteTableResourceRow> RouteTables { get; set; } = new();
    public List<InstanceResourceRow> Instances { get; set; } = new();
}
