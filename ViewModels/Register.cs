using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace WebApplication1.ViewModels
{
	public class Register
	{
        [Required(ErrorMessage = "First Name is required")]
        public string FirstName { get; set; }


        [Required(ErrorMessage = "Last Name is required")] 
		public string LastName { get; set; }


		[Required(ErrorMessage = "Gender is required")] 
		public string Gender { get; set; }


		[Required(ErrorMessage = "NRIC is required")] 
		public string NRIC { get; set; }


		[Required(ErrorMessage = "Email is required")]
		[DataType(DataType.EmailAddress)] 
		public string Email { get; set; }


        [Required(ErrorMessage = "Password is required")]
		[DataType(DataType.Password)] 
		public string Password { get; set; }

		[Required(ErrorMessage = "Confirm Password is required")]
		[DataType(DataType.Password)]
		[Compare(nameof(Password), ErrorMessage = "Password and confirmation password do not match.")]
		public string ConfirmPassword { get; set; }


		[Required(ErrorMessage = "Date of Birth is required")]
		[DataType(DataType.Date)] 
		public DateTime DateOfBirth { get; set; }

		public IFormFile Resume { get; set; } // Change type to IFormFile
		[Required] public string WhoAmI { get; set; }
	}
}

