# VMart - Dotnet MVC E-Commerce 🛒

VMart is a full-featured ASP.NET Core MVC e-commerce web application built with Razor Views, Entity Framework Core, Identity for user management, and integrations like Cloudinary (for image uploads) and Stripe (for payments).


## Prerequisites
.NET SDK 8.0+
SQL Server (LocalDB or Azure SQL)
SMTP setup
Cloudinary Setup
Stripe Account


## 🔧 Configuration

Before running the application, set up your `appsettings.json` file at the root of the project:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "EmailSettings": {
    "SmtpServer": "",
    "SmtpPort": "",
    "FromEmail": "",
    "FromPassword": "",
    "DisplayName": "",
    "AdminEmail": ""
  },
  "Cloudinary": {
    "CloudName": "",
    "ApiKey": "",
    "ApiSecret": ""
  },
  "ConnectionStrings": {
    "DefaultConnection": ""
  },
  "Stripe": {
    "SecretKey": "",
    "PublishableKey": ""
  }
}
```

## 
- Add a migration (only once or as needed)
dotnet ef migrations add InitialCreate or add-migration Initial-migration(for vs package-manager-console)

- Apply migration to create the database
dotnet ef database update or update-database(for vs package-manager-console)


# Run the application
```
dotnet run

