using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace WebApplication1.Services
{
    public class DummyEmailSender : IEmailSender
    {
        private readonly ILogger<DummyEmailSender> _logger;

        public DummyEmailSender(ILogger<DummyEmailSender> logger)
        {
            _logger = logger;
        }

        public Task SendEmailAsync(string email, string subject, string message)
        {
            _logger.LogInformation($"Sending email to {email} with subject '{subject}'. Message: {message}");
            return Task.CompletedTask;
        }
    }
}
