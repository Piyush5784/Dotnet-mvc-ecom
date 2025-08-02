using Microsoft.AspNetCore.Identity;
using VMart.Services;
using VMart.Utility;

namespace VMart.Middleware
{
    public class JwtTokenMiddleware
    {
        private readonly RequestDelegate _next;

        public JwtTokenMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, UserManager<IdentityUser> userManager, JwtTokenService jwtTokenService)
        {
            if (context.User.Identity?.IsAuthenticated == true)
            {
                // Check if we already have a token for this user session
                var existingToken = context.Session.GetString("token");
                var currentUserId = context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                var sessionUserId = context.Session.GetString("userId");

                // Generate new token if:
                // 1. No token exists in session
                // 2. User ID changed (different user logged in)
                if (string.IsNullOrEmpty(existingToken) || sessionUserId != currentUserId)
                {
                    try
                    {
                        var user = await userManager.GetUserAsync(context.User);
                        if (user != null)
                        {
                            var jwtToken = await jwtTokenService.GenerateJwtTokenAsync(user);

                            // Store token and user ID in session
                            context.Session.SetString("token", jwtToken);
                            context.Session.SetString("userId", user.Id);

                            Console.WriteLine($"New JWT token generated and stored for user: {user.UserName}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Failed to generate JWT token: {ex.Message}");
                    }
                }
            }
            else
            {
                // User is not authenticated, clear session tokens
                if (context.Session.Keys.Contains("token") || context.Session.Keys.Contains("userId"))
                {
                    context.Session.Remove("token");
                    context.Session.Remove("userId");
                    Console.WriteLine("Session tokens cleared - user not authenticated");
                }
            }

            await _next(context);
        }
    }

    // Extension method for easy registration
    public static class JwtTokenMiddlewareExtensions
    {
        public static IApplicationBuilder UseJwtTokenMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<JwtTokenMiddleware>();
        }
    }
}