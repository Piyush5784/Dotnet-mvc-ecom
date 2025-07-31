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
            // Check if user is authenticated but doesn't have a JWT token
            if (context.User.Identity?.IsAuthenticated == true)
            {
                var existingToken = context.Session.GetString("token");
                var shouldGenerateNewToken = false;

                // Check if token exists and is valid
                if (string.IsNullOrEmpty(existingToken))
                {
                    shouldGenerateNewToken = true;
                }
                else
                {
                    // Validate existing token
                    var principal = jwtTokenService.ValidateJwtToken(existingToken);
                    if (principal == null)
                    {
                        shouldGenerateNewToken = true;
                    }
                    else
                    {

                        // Verify token has required roles using SD class
                        var tokenRoles = principal.FindAll(System.Security.Claims.ClaimTypes.Role).Select(c => c.Value).ToList();

                        // Ensure token has valid roles from SD class
                        if (!tokenRoles.Any(r => r == SD.Role_Admin || r == SD.Role_User))
                        {
                            shouldGenerateNewToken = true;
                        }
                    }
                }

                // Generate new token if needed
                if (shouldGenerateNewToken)
                {
                    try
                    {
                        var user = await userManager.GetUserAsync(context.User);
                        if (user != null)
                        {
                            var jwtToken = await jwtTokenService.GenerateJwtTokenAsync(user);
                            context.Session.SetString("token", jwtToken);


                            // Log user roles for debugging using SD class constants
                            var roles = await userManager.GetRolesAsync(user);
                            var validRoles = roles.Where(r => r == SD.Role_Admin || r == SD.Role_User).ToList();

                            if (!validRoles.Any())
                            {
                                Console.WriteLine($"⚠️  WARNING: User {user.UserName} has no valid roles assigned");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Failed to generate JWT token: {ex.Message}");
                    }
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
