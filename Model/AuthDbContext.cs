using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Model;

namespace WebApplication1.Model
{
	public class AuthDbContext : IdentityDbContext<ApplicationUser>
	{
		private readonly IConfiguration _configuration;

		public AuthDbContext(DbContextOptions<AuthDbContext> options, IConfiguration configuration)
			: base(options)
		{
			_configuration = configuration;
		}

        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<PasswordHistory> PasswordHistories { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			string connectionString = _configuration.GetConnectionString("AuthConnectionString");
			optionsBuilder.UseSqlServer(connectionString);
		}
	}
}
