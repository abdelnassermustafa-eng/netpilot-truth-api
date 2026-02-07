namespace TruthApi.Models;

public class SubnetDto
{
    public string SubnetId { get; set; } = "";
    public string VpcId { get; set; } = "";
    public string Cidr { get; set; } = "";
    public string AvailabilityZone { get; set; } = "";
    public string State { get; set; } = "";
}

