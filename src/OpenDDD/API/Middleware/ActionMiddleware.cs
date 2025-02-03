using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using OpenDDD.Domain.Model;
using OpenDDD.Infrastructure.Persistence.UoW;
using OpenDDD.Infrastructure.TransactionalOutbox;

namespace OpenDDD.API.Middleware
{
    public class ActionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ActionMiddleware> _logger;

        public ActionMiddleware(RequestDelegate next, ILogger<ActionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, 
            IUnitOfWork unitOfWork, 
            IDomainPublisher domainPublisher, 
            IIntegrationPublisher integrationPublisher, 
            IOutboxRepository outboxRepository)
        {
            var ct = context.RequestAborted;

            try
            {
                _logger.LogDebug("Executing action for request: {RequestPath}", context.Request.Path);

                await unitOfWork.StartAsync(ct);
                await _next(context);

                foreach (var domainEvent in domainPublisher.GetPublishedEvents())
                {
                    _logger.LogDebug("Saving domain event to outbox: {EventType}", domainEvent.GetType().Name);
                    await outboxRepository.SaveEventAsync(domainEvent, ct);
                }

                foreach (var integrationEvent in integrationPublisher.GetPublishedEvents())
                {
                    _logger.LogDebug("Saving integration event to outbox: {EventType}", integrationEvent.GetType().Name);
                    await outboxRepository.SaveEventAsync(integrationEvent, ct);
                }

                await unitOfWork.CommitAsync(ct);

                _logger.LogDebug("Action execution completed successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Transaction failed, rolling back.");
                await unitOfWork.RollbackAsync(ct);
                throw;
            }
        }
    }
}
