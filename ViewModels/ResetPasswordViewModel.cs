using System.ComponentModel.DataAnnotations;

namespace WebApplication1.ViewModels
{
	public class ResetPasswordViewModel
	{
		[Required]
		[EmailAddress]
		public string Email { get; set; }

		[Required]
		public string Token { get; set; }

		[Required(ErrorMessage = "New password is required")]
		[StringLength(100, MinimumLength = 12, ErrorMessage = "Password must be at least 12 characters long")]
		[DataType(DataType.Password)]
		public string NewPassword { get; set; }

		[Required(ErrorMessage = "Confirm password is required")]
		[DataType(DataType.Password)]
		[Compare("NewPassword", ErrorMessage = "New password and confirmation do not match")]
		public string ConfirmPassword { get; set; }
	}
}
