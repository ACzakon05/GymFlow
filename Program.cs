using GymFlow.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// ========== DODANIE USŁUG DO KONTENERA ==========

// Entity Framework Core - SQLite
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? "Data Source=gymflow.db";

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString)
);

// Authentication - Cookies
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login";
        options.LogoutPath = "/Auth/Logout";
        options.AccessDeniedPath = "/Auth/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromDays(30);
        options.SlidingExpiration = true;
    });

// Authorization
builder.Services.AddAuthorization();

// MVC Controllers with Views
builder.Services.AddControllersWithViews();

// ========== BUDOWANIE APLIKACJI ==========

var app = builder.Build();

// ========== KONFIGURACJA HTTP PIPELINE ==========

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseStaticFiles();

app.UseRouting();

// Authentication i Authorization - MUSZĄ być w tej kolejności!
app.UseAuthentication();
app.UseAuthorization();

// ========== ROUTING ==========

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

app.Run();
