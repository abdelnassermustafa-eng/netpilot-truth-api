namespace TruthApi.Models
{
    public class InternetGatewayResourceRow
    {
        public string IgwId { get; set; } = "";
        public string AttachedVpcIds { get; set; } = "";
        public string AttachmentState { get; set; } = "";
        public string Region { get; set; } = "";
    }
}
