using CloudinaryDotNet;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Stripe;
using VMart.Data;
using VMart.Interfaces;
using VMart.Models;
using VMart.Services;
using VMart.Utility;
var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'ApplicationDbContextConnection' not found.");



builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(connectionString));

builder.Services.AddScoped<IEmailSender, EmailSender>();
builder.Services.AddScoped<IEmailSenderApplicationInterface , EmailSenderApplication>();

builder.Services.Configure<CloudinarySettings>(
    builder.Configuration.GetSection("Cloudinary"));

builder.Services.AddSingleton<ICloudinaryService, CloudinaryService>();
builder.Services.AddScoped<ILogService , LogService>();

builder.Services.AddIdentity<IdentityUser, IdentityRole>
    ()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();


builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

builder.Services.Configure<StripeSettings>(builder.Configuration.GetSection("Stripe"));
StripeConfiguration.ApiKey = builder.Configuration["Stripe:SecretKey"];

builder.Services.ConfigureApplicationCookie(o =>
{
    o.LoginPath = $"/Identity/Account/Login";
    o.LogoutPath = $"/Identity/Account/Logout";
    o.AccessDeniedPath = $"/Identity/Account/AccessDenied";
    
});


var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var exceptionFeature = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerPathFeature>();
        var logger = context.RequestServices.GetRequiredService<ILogService>();

        var error = exceptionFeature?.Error;
        var path = exceptionFeature?.Path;
        var user = context.User.Identity?.Name;

        //if (error != null)
        //{
        //    await logger.LogAsync(
        //        SD.Log_Error,
        //        "Unhandled exception",
        //        controller: null,
        //        action: null,
        //        stackTrace: error.ToString(),
        //        path: path,
        //        userName: user
        //    );
        //}

        context.Response.Redirect("/Home/Error");
    });
});


app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
