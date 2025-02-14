using System.ComponentModel.DataAnnotations;

namespace WebApplication1.ViewModels
{
    public class EmailTwoFactorViewModel
    {
        [Required(ErrorMessage = "Security code is required.")]
        public string Code { get; set; }
    }
}
