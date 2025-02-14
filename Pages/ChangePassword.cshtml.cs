using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApplication1.Model;
using WebApplication1.ViewModels;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace WebApplication1.Pages
{
    public class ChangePasswordModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly AuthDbContext _context;
        private readonly IPasswordHasher<ApplicationUser> _passwordHasher;

        private readonly TimeSpan _minPasswordAge = TimeSpan.FromMinutes(1);
        public ChangePasswordModel(UserManager<ApplicationUser> userManager,
                                   SignInManager<ApplicationUser> signInManager,
                                   AuthDbContext context,
                                   IPasswordHasher<ApplicationUser> passwordHasher)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
            _passwordHasher = passwordHasher;
        }

        [BindProperty]
        public ChangePasswordViewModel ChangeModel { get; set; }

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToPage("/Login");
            }

            // Enforce the minimum password age:
            if (user.LastPasswordChangeDate.HasValue &&
                (DateTime.UtcNow - user.LastPasswordChangeDate.Value) < _minPasswordAge)
            {
                ModelState.AddModelError(string.Empty, "You cannot change your password so soon after the last change. Please try again later.");
                return Page();
            }

            // Verify current password
            if (!await _userManager.CheckPasswordAsync(user, ChangeModel.CurrentPassword))
            {
                ModelState.AddModelError(string.Empty, "The current password is incorrect.");
                return Page();
            }

            // Check if new password is the same as current password
            if (_passwordHasher.VerifyHashedPassword(user, user.PasswordHash, ChangeModel.NewPassword) == PasswordVerificationResult.Success)
            {
                ModelState.AddModelError(string.Empty, "New password cannot be the same as the current password.");
                return Page();
            }

            // Retrieve the last 2 password history records for the user
            var previousPasswords = _context.PasswordHistories
                .Where(ph => ph.UserId == user.Id)
                .OrderByDescending(ph => ph.CreatedDate)
                .Take(2)
                .ToList();

            foreach (var history in previousPasswords)
            {
                var verificationResult = _passwordHasher.VerifyHashedPassword(user, history.PasswordHash, ChangeModel.NewPassword);
                if (verificationResult == PasswordVerificationResult.Success)
                {
                    ModelState.AddModelError(string.Empty, "New password cannot be the same as one of your last two passwords.");
                    return Page();
                }
            }

            // Store the current password hash before changing it
            var currentPasswordHash = user.PasswordHash;

            // Change the password
            var changePasswordResult = await _userManager.ChangePasswordAsync(user, ChangeModel.CurrentPassword, ChangeModel.NewPassword);
            if (!changePasswordResult.Succeeded)
            {
                foreach (var error in changePasswordResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return Page();
            }

            // Save the old password hash in history
            var passwordHistory = new PasswordHistory
            {
                UserId = user.Id,
                PasswordHash = currentPasswordHash,
                CreatedDate = DateTime.UtcNow
            };
            _context.PasswordHistories.Add(passwordHistory);
            await _context.SaveChangesAsync();

            // If more than 2 history records exist, remove the oldest ones
            var allHistory = _context.PasswordHistories
                .Where(ph => ph.UserId == user.Id)
                .OrderByDescending(ph => ph.CreatedDate)
                .ToList();
            if (allHistory.Count > 2)
            {
                var toRemove = allHistory.Skip(2);
                _context.PasswordHistories.RemoveRange(toRemove);
                await _context.SaveChangesAsync();
            }

           // Update the last password change date
            user.LastPasswordChangeDate = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);

            // Re-sign in the user
            await _signInManager.RefreshSignInAsync(user);
            return RedirectToPage("Index");
        }
    }
}
