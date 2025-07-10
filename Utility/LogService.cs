using VMart.Data;
using VMart.Interfaces;
using VMart.Models;

namespace VMart.Utility
{


public class LogService : ILogService
{
    private readonly ApplicationDbContext db;

    public LogService(ApplicationDbContext context)
    {
        db = context;
    }

    public async Task LogAsync(string Type, string message, string? controller = null, string? action = null,
                               string? stackTrace = null, string? path = null, string? userName = null)
    {
        var log = new Logs
        {
            Type = Type,
            Message = message,
            Controller = controller,
            Action = action,
            StackTrace = stackTrace,
            RequestPath = path,
            UserName = userName
        };

        db.Logs.Add(log);
        await db.SaveChangesAsync();
    }
}
}