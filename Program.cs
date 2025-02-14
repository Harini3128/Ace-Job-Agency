using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using WebApplication1.Model;
using Microsoft.AspNetCore.Http;
using static System.Runtime.InteropServices.JavaScript.JSType;
using WebApplication1.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddDbContext<AuthDbContext>();
builder.Services.AddTransient<IEmailSender, SmtpEmailSender>();

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 12;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;

    // Configure account lockout
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(2);
    options.Lockout.MaxFailedAccessAttempts = 3;
    options.Lockout.AllowedForNewUsers = true;
})
.AddEntityFrameworkStores<AuthDbContext>()
.AddDefaultTokenProviders();

builder.Services.AddAuthentication("MyCookieAuth").AddCookie("MyCookieAuth", options =>
{
    options.Cookie.Name = "MySecureAppCookie";
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Lax; 
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.ExpireTimeSpan = TimeSpan.FromMinutes(2);
    options.SlidingExpiration = true;
    options.LoginPath = "/Login";
});

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(2); // Session timeout duration
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true; // Ensure the cookie is always sent
});


var app = builder.Build();


app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseStatusCodePagesWithRedirects("/errors/{0}");
app.UseRouting();
app.UseMiddleware<SessionValidationMiddleware>();
app.UseAuthentication();
app.UseAuthorization();
app.UseSession();
app.MapRazorPages();
app.Run();
