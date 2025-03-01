using Microsoft.Extensions.Caching.Memory;
using SIDAPI.Models;
using System.Net;

namespace SIDAPI.Middlewares
{
    public class RateLimitingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IMemoryCache _cache;
        private readonly ILogger<RateLimitingMiddleware> _logger;
        private readonly int _requestLimit = 5;  // Max requests allowed
        private readonly TimeSpan _timeWindow = TimeSpan.FromMinutes(1); // Reset time

        public RateLimitingMiddleware(RequestDelegate next, IMemoryCache cache, ILogger<RateLimitingMiddleware> logger)
        {
            _next = next;
            _cache = cache;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            string key = GetClientKey(context);

            if (string.IsNullOrEmpty(key))
            {
                await _next(context); // If key is empty (rare case), proceed
                return;
            }

            if (_cache.TryGetValue(key, out RateLimitEntry entry))
            {
                if (entry.Timestamp.Add(_timeWindow) > DateTime.UtcNow)
                {
                    if (entry.Count >= _requestLimit)
                    {
                        context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                        context.Response.Headers["Retry-After"] = _timeWindow.TotalSeconds.ToString();
                        await context.Response.WriteAsync("Rate limit exceeded. Try again later.");
                        _logger.LogWarning($"Rate limit exceeded for {key}");
                        return;
                    }
                    entry.Count++;
                }
                else
                {
                    entry.Count = 1;
                    entry.Timestamp = DateTime.UtcNow;
                }
            }
            else
            {
                entry = new RateLimitEntry { Count = 1, Timestamp = DateTime.UtcNow };
            }

            _cache.Set(key, entry, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = _timeWindow
            });

            await _next(context);
        }

        private string GetClientKey(HttpContext context)
        {
            if (context.Request.Headers.ContainsKey("X-API-Key"))
            {
                return "APIKEY_" + context.Request.Headers["X-API-Key"].ToString();
            }

            var ipAddress = context.Connection.RemoteIpAddress;
            if (ipAddress == null || ipAddress.Equals(IPAddress.None))
            {
                return null;
            }

            return "IP_" + ipAddress.ToString();
        }

    }
}
