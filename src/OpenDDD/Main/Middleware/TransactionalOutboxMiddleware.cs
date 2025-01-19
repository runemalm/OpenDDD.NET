using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace OpenDDD.Main.Middleware
{
    public class TransactionalOutboxMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<TransactionalOutboxMiddleware> _logger;

        public TransactionalOutboxMiddleware(RequestDelegate next, ILogger<TransactionalOutboxMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            _logger.LogInformation("Transactional Outbox Middleware Invoked");
            await _next(context);
        }
    }
}
