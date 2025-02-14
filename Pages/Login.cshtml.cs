using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using WebApplication1.ViewModels;
using WebApplication1.Model;
using Newtonsoft.Json;
using IEmailSender = WebApplication1.Services.IEmailSender;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace WebApplication1.Pages
{
    public class LoginModel : PageModel
    {
        [BindProperty]
        public Login LModel { get; set; }

        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly AuthDbContext _context;
		private readonly IConfiguration _configuration;
        private readonly IEmailSender _emailSender;

        public LoginModel(SignInManager<ApplicationUser> signInManager,
                            UserManager<ApplicationUser> userManager,
                            AuthDbContext context, 
                            IConfiguration configuration,
                            IEmailSender emailSender)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _context = context;
			_configuration = configuration;
            _emailSender = emailSender;
        }

        private async Task<bool> ValidateReCaptcha(string token)
        {
            string secretKey = _configuration["ReCaptcha:SecretKey"];

            using (var httpClient = new HttpClient())
            {
                var response = await httpClient.PostAsync(
                    $"https://www.google.com/recaptcha/api/siteverify?secret={secretKey}&response={token}",
                    null);
                var jsonString = await response.Content.ReadAsStringAsync();
                Console.WriteLine("reCAPTCHA response: " + jsonString); // Log the raw response
                dynamic json = JsonConvert.DeserializeObject(jsonString);

                // For testing, temporarily lower threshold:
                return json.success == true && json.score >= 0.3;
            }
        }

		public async Task<IActionResult> OnPostAsync()
        {
            if (ModelState.IsValid)
            {
				var reCaptchaToken = Request.Form["g-recaptcha-response"];
				if (!await ValidateReCaptcha(reCaptchaToken))
				{
					ModelState.AddModelError("", "reCAPTCHA validation failed. Please try again.");
					return Page();
				}

				var user = await _signInManager.UserManager.FindByEmailAsync(LModel.Email);
                if (user != null)
                {
                    var result = await _signInManager.PasswordSignInAsync(user, LModel.Password, LModel.RememberMe, lockoutOnFailure: true);

                    var logger = new AuditLogger(_context);

                    if (result.RequiresTwoFactor)
                    {
                        // Generate the token using the default email token provider
                        var token = await _userManager.GenerateTwoFactorTokenAsync(user, TokenOptions.DefaultEmailProvider);

                        // Send the token via email (using your email sender service)
                        await _emailSender.SendEmailAsync(user.Email, "Your Security Code",
                            $"Your two-factor authentication code is: {token}");

                        // Store the user ID in TempData to retrieve it in the verification page
                        TempData["UserId"] = user.Id;
                        return RedirectToPage("/Account/EmailTwoFactor", new { userId = user.Id });
                    }
                    else if (result.Succeeded)
                    {
                        // Log successful login
                        await logger.LogAsync(user.Id, "User logged in successfully.");
                        Console.WriteLine("Login successful.");

                        // Generate a new session token
                        user.SessionToken = Guid.NewGuid().ToString();
                        await _signInManager.UserManager.UpdateAsync(user);

                        await _signInManager.UserManager.SetAuthenticationTokenAsync(user, "MyApp", "SessionToken", user.SessionToken);

                        // Create a security context with the session token
                        var claims = new List<Claim>
                        {
                            new Claim(ClaimTypes.Name, user.UserName),
                            new Claim(ClaimTypes.Email, user.Email),
                            new Claim("SessionToken", user.SessionToken)
                        };

                        var identity = new ClaimsIdentity(claims, "MyCookieAuth");
                        ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal(identity);
                        await HttpContext.SignInAsync("MyCookieAuth", claimsPrincipal);

                        HttpContext.Session.SetString("UserId", user.Id);
                        HttpContext.Session.SetString("Username", user.UserName);

                        return RedirectToPage("Index");
                    }
                    else if (result.IsLockedOut)
                    {
                        await logger.LogAsync(user.Id, "User account locked out.");
                        Console.WriteLine("User is locked out.");
                        ModelState.AddModelError("", "Your account is locked. Please try again after 2 minutes.");
                    }     
                    else
                    {
                        await logger.LogAsync(user.Id, "Failed login attempt.");
                        Console.WriteLine("Invalid login attempt.");
                        var remainingAttempts = _signInManager.Options.Lockout.MaxFailedAccessAttempts - user.AccessFailedCount;
                        ModelState.AddModelError("", $"Invalid login attempt. You have {remainingAttempts} more attempt(s) before your account is locked.");
                    }
                }
            }

			return Page();
        }
		public void OnGet()
        {
        }
    }
}
