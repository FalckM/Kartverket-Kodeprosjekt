using FirstWebApplication.Data;
using FirstWebApplication.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Henter connection string fra appsettings.json eller User Secrets
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Registrerer DbContext som en tjeneste
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(connectionString, new MariaDbServerVersion(new Version(10, 11, 0))));

// AddIdentity registrerer alle nødvendige tjenester for brukerautentisering
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    // Passordkrav - gjør det enkelt for testing (kan strenges til senere!)
    options.Password.RequireDigit = false;           
    options.Password.RequireLowercase = false;        
    options.Password.RequireUppercase = false;        
    options.Password.RequireNonAlphanumeric = false;  
    options.Password.RequiredLength = 6;              

    // Brukerinnstillinger
    options.User.RequireUniqueEmail = true;           
})
.AddEntityFrameworkStores<ApplicationDbContext>()    
.AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    // Redirect to login page when user is not authenticated
    options.LoginPath = "/Account/AuthPage";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AuthPage";
});

var app = builder.Build();


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
    pattern: "{controller=Account}/{action=AuthPage}/{id?}")
    .WithStaticAssets();

app.Run();