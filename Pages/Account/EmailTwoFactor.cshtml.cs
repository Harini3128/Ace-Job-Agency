using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Threading.Tasks;
using WebApplication1.Model;
using WebApplication1.ViewModels;
using WebApplication1.Services;

namespace WebApplication1.Pages.Account
{
    public class EmailTwoFactorModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IEmailSender _emailSender;

        public EmailTwoFactorModel(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IEmailSender emailSender)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
        }

        [BindProperty]
        public EmailTwoFactorViewModel Input { get; set; }

        public string Message { get; set; }

        public void OnGet() { }

        // Handler for verifying the token
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Retrieve user id from TempData
            var userId = TempData["UserId"] as string;
            if (string.IsNullOrEmpty(userId))
            {
                ModelState.AddModelError("", "User session expired. Please log in again.");
                return RedirectToPage("/Login");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                ModelState.AddModelError("", "Invalid user. Please try again.");
                return RedirectToPage("/Login");
            }

            // Normalize the code
            var verificationCode = Input.Code.Replace(" ", string.Empty).Replace("-", string.Empty);

            // Verify the token using the default email token provider
            var result = await _userManager.VerifyTwoFactorTokenAsync(user, TokenOptions.DefaultEmailProvider, verificationCode);
            if (!result)
            {
                ModelState.AddModelError("", "Invalid security code.");
                return Page();
            }

            // Enable two-factor authentication for the user if not already enabled.
            if (!user.TwoFactorEnabled)
            {
                await _userManager.SetTwoFactorEnabledAsync(user, true);
            }

            // Complete sign-in
            await _signInManager.SignInAsync(user, isPersistent: false);
            return RedirectToPage("/Index");
        }

        // New handler for resending the code
        public async Task<IActionResult> OnPostResendAsync()
        {
            // Retrieve user id from TempData
            var userId = TempData["UserId"] as string;
            if (string.IsNullOrEmpty(userId))
            {
                ModelState.AddModelError("", "User session expired. Please log in again.");
                return RedirectToPage("/Login");
            }

            // Preserve the UserId in TempData for subsequent requests
            TempData.Keep("UserId");

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                ModelState.AddModelError("", "Invalid user. Please log in again.");
                return RedirectToPage("/Login");
            }

            // Generate a new token using the default email token provider
            var token = await _userManager.GenerateTwoFactorTokenAsync(user, TokenOptions.DefaultEmailProvider);

            // Send the token via email
            await _emailSender.SendEmailAsync(user.Email, "Your Security Code",
                $"Your two-factor authentication code is: {token}");

            // Optionally, set a message to show that a new code has been sent.
            Message = "A new security code has been sent to your email.";

            // Keep TempData["UserId"] for the next request and return to the same page
            return Page();
        }
    }
}
