using FirstWebApplication.Data;
using FirstWebApplication.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllersWithViews();

// Configure database connection
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

builder.Services.AddScoped<DatabaseSeeder>();

// Configure ASP.NET Identity with roles
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    // Password settings
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Register services
builder.Services.AddScoped<UserRoleService>();
builder.Services.AddScoped<RoleInitializerService>();
builder.Services.AddScoped<UserSeederService>();

// Configure cookie settings
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Home/Index";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Home/Index";
    options.ExpireTimeSpan = TimeSpan.FromHours(24);
    options.SlidingExpiration = true;
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    options.Cookie.SameSite = SameSiteMode.Lax;
});

var app = builder.Build();

// Initialize roles and seed test users at startup
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    // Create the three roles (Pilot, Registerfører, Admin)
    var roleInitializer = services.GetRequiredService<RoleInitializerService>();
    await roleInitializer.InitializeRolesAsync();

    // Seed test users (pilot@test.com, registerforer@test.com, admin@test.com)
    // Only runs in Development environment
    if (app.Environment.IsDevelopment())
    {
        var userSeeder = services.GetRequiredService<UserSeederService>();
        await userSeeder.SeedTestUsersAsync();
    }
}

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Authentication must come before Authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();