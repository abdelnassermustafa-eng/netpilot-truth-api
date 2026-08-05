namespace TruthApi.Models.Aws.LoadBalancing;

public sealed class AwsForwardTargetGroupInfo
{
    public string TargetGroupArn { get; init; } = "";

    public int? Weight { get; init; }
}
