using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using OpenDDD.Domain.Model;
using OpenDDD.Infrastructure.Persistence.UoW;
using OpenDDD.Infrastructure.TransactionalOutbox;

namespace OpenDDD.Main.Middleware
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
                await unitOfWork.StartAsync(ct);
                await _next(context);

                foreach (var domainEvent in domainPublisher.GetPublishedEvents())
                {
                    await outboxRepository.SaveEventAsync(domainEvent, ct);
                }

                foreach (var integrationEvent in integrationPublisher.GetPublishedEvents())
                {
                    await outboxRepository.SaveEventAsync(integrationEvent, ct);
                }

                await unitOfWork.CommitAsync(ct);
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
