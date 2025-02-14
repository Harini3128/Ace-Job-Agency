using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text;
using System.Threading.Tasks;
using WebApplication1.Model;
using System.Security.Cryptography;

namespace WebApplication1.Pages
{
    public class IndexModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly TimeSpan _maxPasswordAge = TimeSpan.FromDays(90);

        public string Email { get; private set; }
        public string FirstName { get; private set; }
        public string LastName { get; private set; }
        public string Gender { get; private set; }
        public string DateOfBirth { get; private set; }
        public string DecryptedNRIC { get; private set; }

        public IndexModel(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }
        public async Task<IActionResult> OnGetAsync()
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("UserId")))
            {
                Console.WriteLine("Session expired. Redirecting to login.");
                return RedirectToPage("/Login");
            }
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                Console.WriteLine("User not found or session expired.");
                DecryptedNRIC = "User not found or session expired.";
                return RedirectToPage("/Login"); // Redirect if user is null
            }

            // If the user changed their password more than 90 days ago, force a change
            if (user.LastPasswordChangeDate.HasValue &&
                (DateTime.UtcNow - user.LastPasswordChangeDate.Value) > _maxPasswordAge)
            {
                return RedirectToPage("/ChangePassword", new { message = "Your password has expired. Please change your password." });
            }

            Email = user.Email;
            FirstName = user.FirstName;
            LastName = user.LastName;
            Gender = user.Gender;
            DateOfBirth = user.DateOfBirth.ToShortDateString();

            if (!string.IsNullOrEmpty(user.EncryptedNRIC))
            {
                DecryptedNRIC = DecryptNRIC(user.EncryptedNRIC);
                Console.WriteLine("Decrypted NRIC: " + DecryptedNRIC);
            }
            else
            {
                Console.WriteLine("Encrypted NRIC is null or empty.");
            }

            Email = user.Email;
            return Page();
        }

        private string DecryptNRIC(string encryptedData)
        {
            try
            {
                string[] parts = encryptedData.Split(':');
                if (parts.Length != 2)
                    throw new Exception("Invalid encrypted data format.");

                byte[] encryptedBytes = Convert.FromBase64String(parts[0]);
                byte[] iv = Convert.FromBase64String(parts[1]);
                byte[] key = Encoding.UTF8.GetBytes("your_secure_key_here1234"); // Use the same key as in EncryptNRIC

                using (Aes aes = Aes.Create())
                {
                    aes.Key = key;
                    aes.IV = iv;
                    var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
                    byte[] decryptedBytes = decryptor.TransformFinalBlock(encryptedBytes, 0, encryptedBytes.Length);
                    return Encoding.UTF8.GetString(decryptedBytes);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to decrypt NRIC: " + ex.Message);
                return "Failed to decrypt NRIC.";
            }
        }

    }
}
