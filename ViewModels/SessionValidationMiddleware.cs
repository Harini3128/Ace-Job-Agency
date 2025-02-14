using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using WebApplication1.Model;

public class SessionValidationMiddleware
{
    private readonly RequestDelegate _next;

    public SessionValidationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context, UserManager<ApplicationUser> userManager)
    {
        if (context.User.Identity.IsAuthenticated)
        {
            var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var sessionToken = context.User.FindFirstValue("SessionToken");

            if (!string.IsNullOrEmpty(userId) && !string.IsNullOrEmpty(sessionToken))
            {
                var user = await userManager.FindByIdAsync(userId);

                if (user != null && user.SessionToken != sessionToken)
                {
                    // Token mismatch detected
                    await context.SignOutAsync("MyCookieAuth");
                    context.Response.Redirect("/Login?sessionExpired=true");
                    return;
                }
            }
        }

        await _next(context);
    }
}
