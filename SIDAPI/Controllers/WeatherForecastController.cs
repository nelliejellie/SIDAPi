using Hangfire;
using Microsoft.AspNetCore.Mvc;
using SIDAPI.Models;
using SIDAPI.Services;

namespace SIDAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    private static readonly string[] Summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    private readonly ILogger<WeatherForecastController> _logger;
    private readonly IBackgroundJobClient _backgroundJobClient;

    public WeatherForecastController(ILogger<WeatherForecastController> logger, IBackgroundJobClient backgroundJobClient)
    {
        _logger = logger;
        _backgroundJobClient = backgroundJobClient;
    }

    [HttpGet(Name = "GetWeatherForecast")]
    public IEnumerable<WeatherForecast> Get()
    {
        return Enumerable.Range(1, 5).Select(index => new WeatherForecast
        {
            Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            TemperatureC = Random.Shared.Next(-20, 55),
            Summary = Summaries[Random.Shared.Next(Summaries.Length)]
        })
        .ToArray();
    }

    [HttpPost("sendmail")]
    public IActionResult SendEmail([FromBody] EmailRequest request)
    {
        _backgroundJobClient.Enqueue<EmailService>(emailService =>
            emailService.SendEmailAsync(request.Recipient, request.Subject, request.Body));

        return Ok("? Email job added to the background queue!");
    }
}
