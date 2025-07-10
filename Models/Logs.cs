namespace VMart.Models
{
    public class Logs
    {
        public int Id { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string Type { get; set; } 
        public string Message { get; set; } 
        public string? Controller { get; set; }
        public string? Action { get; set; }

        public string? UserName { get; set; }
        public string? StackTrace { get; set; }
        public string? RequestPath { get; set; }

    }
}
