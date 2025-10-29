using FirstWebApplication.Data;
using FirstWebApplication.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Henter connection string fra appsettings.json eller User Secrets
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Registrerer DbContext som en tjeneste
//builder.Services.AddDbContext<ApplicationDbContext>(options =>
//options.UseMySql(connectionString, new MariaDbServerVersion(new Version(10, 11, 0))));

// FORCE FAIL IF CONNECTION INVALID
if (string.IsNullOrEmpty(connectionString))
{
    throw new Exception("NO CONNECTION STRING FOUND!");
}

if (!connectionString.Contains("3306"))
{
    throw new Exception($"WRONG CONNECTION STRING: {connectionString}");
}

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseMySql(connectionString, new MariaDbServerVersion(new Version(10, 11, 0)));
    Console.WriteLine($"=== MYSQL CONFIGURED: {connectionString} ===");
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var connString = db.Database.GetConnectionString();
    var provider = db.Database.ProviderName;

    Console.WriteLine($"======================================");
    Console.WriteLine($"Provider: {provider}");
    Console.WriteLine($"Connection: {connString}");

    // Hvis SQLite, vis hvor filen er
    if (provider?.Contains("Sqlite") == true)
    {
        Console.WriteLine($"SQLITE DATABASE FILE LOCATION!");
    }
    Console.WriteLine($"======================================");
}

// Debug dersom migration feiler
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();

    try
    {
        var db = services.GetRequiredService<ApplicationDbContext>();
        db.Database.Migrate();
        logger.LogInformation("Database migrations completed successfully");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while migrating the database");
        // Appen fortsetter � kj�re selv om migrations feiler
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();