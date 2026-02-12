namespace TruthApi.Models;

public class InstanceResourceRow
{
    public string InstanceId { get; set; } = "";
    public string InstanceType { get; set; } = "";
    public string State { get; set; } = "";
    public string SubnetId { get; set; } = "";
    public string VpcId { get; set; } = "";
    public string AvailabilityZone { get; set; } = "";
    public string Region { get; set; } = "";
}
