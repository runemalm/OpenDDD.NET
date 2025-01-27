using Microsoft.AspNetCore.Http;
using OpenDDD.Infrastructure.Persistence.UoW;

namespace OpenDDD.Main.Middleware
{
    public class UnitOfWorkMiddleware
    {
        private readonly RequestDelegate _next;

        public UnitOfWorkMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, IUnitOfWork unitOfWork)
        {
            var ct = context.RequestAborted;

            try
            {
                await unitOfWork.BeginTransactionAsync(ct);
                await _next(context);
                await unitOfWork.CommitAsync(ct);
            }
            catch (Exception)
            {
                await unitOfWork.RollbackAsync(ct);
                throw;
            }
        }
    }
}
