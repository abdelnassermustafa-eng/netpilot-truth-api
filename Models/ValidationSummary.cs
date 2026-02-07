namespace TruthApi.Models;

public class ValidationSummary
{
    public int Total { get; set; }
    public int Pass { get; set; }
    public int Fail { get; set; }
    public int Score { get; set; } // 0–100
}

