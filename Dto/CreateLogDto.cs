namespace VMart.Dto
{
    public class CreateLogDto
    {
        public string? Type { get; set; }
        public string? Message { get; set; }
        public string? Controller { get; set; }
        public string? Action { get; set; }
        public string? StackTrace { get; set; }
        public string? RequestPath { get; set; }
        public string? UserName { get; set; }
    }
}
