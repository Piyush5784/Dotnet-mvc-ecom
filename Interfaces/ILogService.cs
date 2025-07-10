namespace VMart.Interfaces
{
    public interface ILogService
    {
        Task LogAsync(
            string Type,
            string message,
            string? controller = null,
            string? action = null,
            string? stackTrace = null,
            string? path = null,
            string? userName = null
     );
    }

}
