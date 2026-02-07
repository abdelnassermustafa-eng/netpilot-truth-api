namespace TruthApi.Models
{
    public class ErrorResponse
    {
        public bool Success { get; set; } = false;
        public string Error { get; set; } = "Internal server error";
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}

