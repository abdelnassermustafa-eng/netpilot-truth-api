namespace TruthApi.Models;

public class RouteTableResourceRow
{
    public string RouteTableId { get; set; } = "";
    public string VpcId { get; set; } = "";
    public bool IsMain { get; set; }
    public string AssociationType { get; set; } = "";
    public string AssociatedSubnetId { get; set; } = "";
    public string DestinationCidr { get; set; } = "";
    public string Region { get; set; } = "";
}
