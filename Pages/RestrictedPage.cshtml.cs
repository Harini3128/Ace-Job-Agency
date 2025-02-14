using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Threading.Tasks;
using WebApplication1.Model;

namespace WebApplication1.Pages
{
    [Authorize]
    public class RestrictedPageModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public RestrictedPageModel(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task OnGet()
        {
            var user = await _userManager.GetUserAsync(User);
            var roles = await _userManager.GetRolesAsync(user);

            foreach (var role in roles)
            {
                Console.WriteLine($"User Role: {role}");
            }
        }
    }
}
