using System;
using System.ComponentModel.DataAnnotations;

namespace WebApplication1.ViewModels
{
    public class ChangePasswordViewModel
    {
        [Required(ErrorMessage = "Current password is required")]
        [DataType(DataType.Password)]
        public string CurrentPassword { get; set; }

        [Required(ErrorMessage = "New password is required")]
        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 12, ErrorMessage = "New password must be at least 12 characters long")]
        public string NewPassword { get; set; }

        [Required(ErrorMessage = "Confirm new password is required")]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "New password and confirmation do not match")]
        public string ConfirmNewPassword { get; set; }
    }
}
