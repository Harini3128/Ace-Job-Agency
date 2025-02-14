using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Threading.Tasks;
using WebApplication1.Model;
using WebApplication1.ViewModels;

namespace WebApplication1.Pages
{
    public class ResetPasswordModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public ResetPasswordModel(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        [BindProperty]
        public ResetPasswordViewModel Input { get; set; }

        public void OnGet(string token = null, string email = null)
        {
            Input = new ResetPasswordViewModel
            {
                Token = token,
                Email = email
            };
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var user = await _userManager.FindByEmailAsync(Input.Email);
            if (user == null)
            {
                // Do not reveal that the user does not exist.
                return RedirectToPage("ResetPasswordConfirmation");
            }

            var result = await _userManager.ResetPasswordAsync(user, Input.Token, Input.NewPassword);
            if (result.Succeeded)
            {
                return RedirectToPage("ResetPasswordConfirmation");
            }
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }
            return Page();
        }
    }
}
