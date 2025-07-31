using CloudinaryDotNet;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Stripe;
using VMart.Data;
using VMart.Interfaces;
using VMart.Middleware;
using VMart.Models;
using VMart.Services;
using VMart.Utility;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");


builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddIdentity<IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddHttpContextAccessor();
builder.Services.AddControllersWithViews();

builder.Services.AddScoped<JwtTokenService>();
builder.Services.AddRazorPages();


builder.Services.Configure<CloudinarySettings>(
    builder.Configuration.GetSection("Cloudinary"));
builder.Services.Configure<StripeSettings>(
    builder.Configuration.GetSection("Stripe"));
StripeConfiguration.ApiKey = builder.Configuration["Stripe:SecretKey"];


builder.Services.AddScoped<IEmailSender, EmailSender>();
builder.Services.AddScoped<IEmailSenderApplicationInterface, EmailSenderApplication>();
builder.Services.AddSingleton<ICloudinaryService, CloudinaryService>();

builder.Services.AddScoped<ILogService, ApiLogService>();

builder.Services.AddHttpClient<ApiClientService>(client =>
{
    client.BaseAddress = new Uri("http://localhost:5202"); // Changed to HTTP port
    client.Timeout = TimeSpan.FromSeconds(30);
});


builder.Services.AddDistributedMemoryCache(); // Required for session
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});


//builder.Services.ConfigureApplicationCookie(options =>
//{
//    options.LoginPath = "/Identity/Account/Login";
//    options.LogoutPath = "/Identity/Account/Logout";
//    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
//});


var app = builder.Build();

// Exception logging
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var exceptionFeature = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerPathFeature>();
        var logger = context.RequestServices.GetRequiredService<ILogService>();

        var error = exceptionFeature?.Error;
        var path = exceptionFeature?.Path;
        var user = context.User.Identity?.Name;

        if (error != null)
        {
            await logger.LogAsync(
                SD.Log_Error,
                "Unhandled exception",
                controller: null,
                action: null,
                stackTrace: error.ToString(),
                path: path,
                userName: user
            );
        }

        context.Response.Redirect("/Home/Error");
    });
});

// -------------------------
// Middleware Pipeline
// -------------------------

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession(); // Must come before UseAuthentication
app.UseAuthentication();
app.UseJwtTokenMiddleware(); // Add JWT token middleware after authentication
app.UseAuthorization();

app.MapRazorPages();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
