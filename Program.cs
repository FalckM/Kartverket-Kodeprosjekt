using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NRLWebApp.Data;
using NRLWebApp.Models.Entities;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultUI() 
    .AddDefaultTokenProviders(); 

builder.Services.AddControllersWithViews();

builder.Services.AddRazorPages();

var app = builder.Build();

// ERSTATT DEN GAMLE 'using (var scope...)' BLOKKEN MED DENNE:
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();

    try
    {
        // Steg 1: Prøv å migrere databasen
        logger.LogInformation("Attempting to migrate database...");
        var dbContext = services.GetRequiredService<ApplicationDbContext>();
        await dbContext.Database.MigrateAsync();
        logger.LogInformation("Database migration successful.");
    }
    catch (Exception ex)
    {
        // Logg KUN migreringsfeil
        logger.LogError(ex, "An error occurred while migrating the database.");
    }

    try
    {
        // Steg 2: Prøv å seede data (kun hvis migrering var vellykket)
        logger.LogInformation("Attempting to seed data...");
        await DataSeeder.Initialize(services);
        logger.LogInformation("Data seeding successful.");
    }
    catch (Exception ex)
    {
        // Logg KUN seeder-feil
        logger.LogError(ex, "An error occurred while seeding the DB.");
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
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

app.MapRazorPages()
   .WithStaticAssets();

app.Run();
