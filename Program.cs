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

// MVC Controllers with Views and API controllers
builder.Services.AddControllersWithViews();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "GymFlow API V1");
});

// ========== ROUTING ==========

app.MapControllers();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var usersWithoutKey = await db.Users
        .Where(u => string.IsNullOrWhiteSpace(u.ApiKey))
        .ToListAsync();

    if (usersWithoutKey.Any())
    {
        foreach (var user in usersWithoutKey)
        {
            user.ApiKey = Guid.NewGuid().ToString();
        }

        await db.SaveChangesAsync();
    }
}

app.Run();


