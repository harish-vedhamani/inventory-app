using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Playground.Middleware
{
    // Catches uncaught exceptions and returns a simple JSON payload.
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }
        public async Task InvokeAsync(HttpContext context)
        {
            const string header = "X-Correlation-ID";

            // ensure a correlation id is present for logging and responses
            string? correlation = null;
            if (context.Request.Headers.TryGetValue(header, out var hv) && !Microsoft.Extensions.Primitives.StringValues.IsNullOrEmpty(hv))
                correlation = hv.ToString();

            if (string.IsNullOrEmpty(correlation))
            {
                correlation = Guid.NewGuid().ToString();
            }

            context.Items[header] = correlation;
            // set header on response so clients can see it
            if (!context.Response.Headers.ContainsKey(header))
                context.Response.Headers[header] = correlation;

            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                // log and return a minimal JSON error; avoid throwing from the error handler
                _logger.LogError(ex, "Unhandled exception (CorrelationId={CorrelationId})", correlation);

                if (!context.Response.HasStarted)
                {
                    context.Response.Clear();
                    context.Response.ContentType = "application/json";
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    var error = new { error = "An unexpected error occurred.", correlationId = correlation };
                    var payload = JsonSerializer.Serialize(error);
                    await context.Response.WriteAsync(payload);
                }
            }
        }
    }
}
