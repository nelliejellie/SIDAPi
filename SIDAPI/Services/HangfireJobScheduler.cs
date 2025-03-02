using Hangfire;

namespace SIDAPI.Services
{
    public class HangfireJobScheduler
    {
        private readonly IRecurringJobManager _recurringJobManager;

        public HangfireJobScheduler(IRecurringJobManager recurringJobManager)
        {
            _recurringJobManager = recurringJobManager;
        }

        public void ScheduleJobs()
        {
            _recurringJobManager.AddOrUpdate<EmailService>(
                "daily-report-job",
                emailService => emailService.SendEmailAsync("admin@example.com", "Daily Report", "Here is your daily report"),
                Cron.Daily);
        }
    }
}
