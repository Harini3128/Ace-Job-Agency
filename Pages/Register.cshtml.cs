using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebApplication1.ViewModels;
using WebApplication1.Model;
using System.IO;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Text;
using System;

namespace WebApplication1.Pages
{
    public class RegisterModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        [BindProperty]
        public Register RModel { get; set; }

        public RegisterModel(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public void OnGet()
        {
        }

		public async Task<IActionResult> OnPostAsync()
        {
            // Custom validation for the Resume file
            if (RModel.Resume == null || RModel.Resume.Length == 0)
            {
                ModelState.AddModelError("RModel.Resume", "Resume file is required.");
            }
            else if (RModel.Resume.Length > 5 * 1024 * 1024) // File size limit: 5MB
            {
                ModelState.AddModelError("RModel.Resume", "File size must not exceed 5 MB.");
            }
            else if (RModel.Resume.ContentType != "application/pdf" &&
                     RModel.Resume.ContentType != "application/vnd.openxmlformats-officedocument.wordprocessingml.document")
            {
                ModelState.AddModelError("RModel.Resume", "Only .pdf and .docx files are allowed.");
            }

            // Stop execution if validation fails
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Encrypt NRIC
            string encryptedNRIC = EncryptNRIC(RModel.NRIC);

            // Ensure the 'wwwroot/resumes' directory exists
            string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/resumes");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            // Generate a unique file name for the uploaded resume
            string uniqueFileName = Guid.NewGuid().ToString() + "_" + RModel.Resume.FileName;
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

            // Save the file to the server
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await RModel.Resume.CopyToAsync(fileStream);
            }

			if (!IsPasswordStrong(RModel.Password))
			{
				ModelState.AddModelError("RModel.Password", "Password must be at least 12 characters long, contain an uppercase letter, lowercase letter, number, and special character.");
				return Page();
			}

			// Create new ApplicationUser
			var user = new ApplicationUser
            {
                UserName = RModel.Email,
                Email = RModel.Email,
                FirstName = RModel.FirstName,
                LastName = RModel.LastName,
                Gender = RModel.Gender,
                EncryptedNRIC = encryptedNRIC,
                DateOfBirth = RModel.DateOfBirth,
                ResumePath = "/resumes/" + uniqueFileName, // Fix: Use uniqueFileName instead of Resume.FileName
                WhoAmI = RModel.WhoAmI
            };

            // Create the user in the database
            var result = await _userManager.CreateAsync(user, RModel.Password);
            if (result.Succeeded)
            {
                await _signInManager.SignInAsync(user, false);
                return RedirectToPage("Index");
            }

            // If user creation fails, display errors
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return Page();
        }

        private string EncryptNRIC(string nric)
        {
            byte[] key = Encoding.UTF8.GetBytes("your_secure_key_here1234"); // Use a secure key
            using (Aes aes = Aes.Create())
            {
                aes.Key = key;
                aes.GenerateIV();
                var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
                byte[] nricBytes = Encoding.UTF8.GetBytes(nric);
                byte[] encrypted = encryptor.TransformFinalBlock(nricBytes, 0, nricBytes.Length);
                return Convert.ToBase64String(encrypted) + ":" + Convert.ToBase64String(aes.IV);
            }
        }

		private bool IsPasswordStrong(string password)
		{
			return password.Length >= 12 &&
				   password.Any(char.IsUpper) &&
				   password.Any(char.IsLower) &&
				   password.Any(char.IsDigit) &&
				   password.Any(ch => !char.IsLetterOrDigit(ch)); // Special character
		}
	}
}
