using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Threading.Tasks;
using WebApplication1.Model;
using WebApplication1.ViewModels;
using WebApplication1.Services;
using Microsoft.AspNetCore.Identity.UI.Services;
using CustomEmailSender = WebApplication1.Services.IEmailSender;

namespace WebApplication1.Pages
{
    public class ForgotPasswordModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly CustomEmailSender _emailSender;

        public ForgotPasswordModel(UserManager<ApplicationUser> userManager, CustomEmailSender emailSender)
        {
            _userManager = userManager;
            _emailSender = emailSender;
        }

        [BindProperty]
        public ForgotPasswordViewModel Input { get; set; }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(Input.Email);
                if (user == null)
                {
                    // To avoid revealing that the email does not exist
                    return RedirectToPage("ForgotPasswordConfirmation");
                }

                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var resetLink = Url.Page(
                    "/ResetPassword",
                    null,
                    new { token, email = Input.Email },
                    Request.Scheme);

                // Send the reset link via email (using a dummy email sender for now)
                await _emailSender.SendEmailAsync(Input.Email, "Reset Password",
                    $"Please reset your password by clicking here: <a href='{resetLink}'>Reset Password</a>");

                return RedirectToPage("ForgotPasswordConfirmation");
            }
            return Page();
        }
    }
}
