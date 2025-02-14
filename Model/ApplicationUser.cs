using Microsoft.AspNetCore.Identity;
using System;

namespace WebApplication1.Model
{
	public class ApplicationUser : IdentityUser
	{
		public string FirstName { get; set; }
		public string LastName { get; set; }
		public string Gender { get; set; }
		public string EncryptedNRIC { get; set; }
		public DateTime DateOfBirth { get; set; }
		public string ResumePath { get; set; }
		public string WhoAmI { get; set; }
		public string? SessionToken { get; set; }
        public DateTime? LastPasswordChangeDate { get; set; }
    }
}
