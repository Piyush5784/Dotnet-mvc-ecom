using VMart.Dto;
using VMart.Services;
using VMart.Interfaces;
using VMart.Utility;
using VMart.Data;
using VMart.Models;
using Microsoft.EntityFrameworkCore;

namespace VMart.Services
{
    /// <summary>
    /// Centralized logging service that sends logs to the Ecom API first,
    /// with automatic fallback to direct database logging if the API is unavailable.
    /// This ensures all application logs are centralized while maintaining reliability.
    /// </summary>
    public class ApiLogService : ILogService
    {
        private readonly ApiClientService _apiClient;
        private readonly ApplicationDbContext _context;

        public ApiLogService(ApiClientService apiClient, ApplicationDbContext context)
        {
            _apiClient = apiClient;
            _context = context;
        }

        public async Task LogAsync(string Type, string message, string? controller = null, string? action = null, string? stackTrace = null, string? path = null, string? userName = null)
        {
            try
            {
                // Try to log via API first
                var logData = new CreateLogDto
                {
                    Type = Type,
                    Message = message,
                    Controller = controller,
                    Action = action,
                    StackTrace = stackTrace,
                    RequestPath = path,
                    UserName = userName
                };

                var apiResponse = await _apiClient.PostAsync<ApiResponseDto<object>>("/api/Log/create", logData);

                if (apiResponse != null && apiResponse.Success)
                {
                    // Successfully logged via API
                    return;
                }

                // If API fails, fallback to local database logging
                Console.WriteLine($"API logging failed, falling back to local database: {apiResponse?.Message}");
                await LogToLocalDatabase(Type, message, controller, action, stackTrace, path, userName);
            }
            catch (Exception ex)
            {
                // If API call throws exception, fallback to local database logging
                Console.WriteLine($"API logging exception, falling back to local database: {ex.Message}");
                try
                {
                    await LogToLocalDatabase(Type, message, controller, action, stackTrace, path, userName);
                }
                catch (Exception localEx)
                {
                    // If even local logging fails, write to console as last resort
                    Console.WriteLine($"All logging failed - Type: {Type}, Message: {message}, LocalError: {localEx.Message}");
                }
            }
        }

        private async Task LogToLocalDatabase(string type, string message, string? controller, string? action, string? stackTrace, string? path, string? userName)
        {
            try
            {
                var log = new Logs
                {
                    Type = type,
                    Message = message,
                    Controller = controller,
                    Action = action,
                    StackTrace = stackTrace,
                    RequestPath = path,
                    UserName = userName,
                    Timestamp = DateTime.UtcNow
                };

                await _context.Logs.AddAsync(log);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Local database logging failed: {ex.Message}");
                throw; // Re-throw to be handled by the calling method
            }
        }
    }
}
