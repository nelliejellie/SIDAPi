
using SIDAPI.Middlewares;
using Serilog;
using Hangfire;
using Hangfire.SqlServer;
using SIDAPI.Services;
using SIDAPI.Data;
using Microsoft.EntityFrameworkCore;

namespace SIDAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Configure Serilog to log into a text file
            Log.Logger = new LoggerConfiguration()
                .WriteTo.File("logs.txt", rollingInterval: RollingInterval.Day)
                .CreateLogger();

            builder.Host.UseSerilog(); // Set Serilog as the logger

            // Add services to the container.

            // Add DbContext using SQL Server
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("HangfireConnection")));

            // Configure Hangfire with the same database
            builder.Services.AddHangfire(config => config.UseSqlServerStorage(
                builder.Configuration.GetConnectionString("HangfireConnection"),
                new SqlServerStorageOptions
                {
                    SchemaName = "Hangfire", // Ensure Hangfire uses its own schema
                    QueuePollInterval = TimeSpan.FromSeconds(15) // Optimize job polling
                }));

            builder.Services.AddHangfireServer(); // Background job server

            // Register Hangfire Job Client and Recurring Job Manager
           
            builder.Services.AddScoped<IRecurringJobManager, RecurringJobManager>();
            builder.Services.AddScoped<HangfireJobScheduler>(); // Register the job scheduler

            builder.Services.AddControllers();
            builder.Services.AddMemoryCache(); // Required for rate limiting
                                               // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Ensure database is created & migrations applied at startup
            using (var scope = app.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                dbContext.Database.Migrate(); // Applies migrations automatically
            }



            // Enable Hangfire Dashboard (UI to monitor jobs)
            app.UseHangfireDashboard();

            // Start processing jobs
            app.MapHangfireDashboard();

            // Middleware for Rate Limiting
            app.UseMiddleware<RateLimitingMiddleware>();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
