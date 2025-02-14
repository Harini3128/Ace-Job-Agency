using System;

namespace WebApplication1.Model
{
    public class PasswordHistory
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string PasswordHash { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
