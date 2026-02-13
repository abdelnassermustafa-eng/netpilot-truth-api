namespace TruthApi.Models;

public class ValidationReport
{
    public ValidationSummary Summary { get; set; } = new();
    public List<ValidationResult> Results { get; set; } = new();

    // ===============================
    // Resource Inventory (Phase 6.x)
    // ===============================
    public List<VpcResourceRow> Vpcs { get; set; } = new();
    public List<SubnetResourceRow> Subnets { get; set; } = new();
    public List<RouteTableResourceRow> RouteTables { get; set; } = new();
    public List<InstanceResourceRow> Instances { get; set; } = new();
    public List<InternetGatewayResourceRow> InternetGateways { get; set; } = new();
}
