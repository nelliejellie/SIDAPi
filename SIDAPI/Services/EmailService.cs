namespace SIDAPI.Services
{
    public class EmailService
    {
        private readonly ILogger<EmailService> _logger;

        public EmailService(ILogger<EmailService> logger)
        {
            _logger = logger;
        }

        public async Task SendEmailAsync(string recipient, string subject, string body)
        {
            // Simulate email sending
            _logger.LogInformation($"📧 Sending email to {recipient}: {subject}");
            await Task.Delay(2000); // Simulate delay
            _logger.LogInformation("✅ Email sent successfully!");
        }
    }
}
