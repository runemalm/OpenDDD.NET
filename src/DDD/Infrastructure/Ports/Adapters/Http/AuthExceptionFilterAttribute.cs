using System.Net;
using System.Net.Http;
using System.Web.Http.Filters;
using DDD.Domain.Auth.Exceptions;

namespace DDD.Infrastructure.Ports.Adapters.Http
{
    public class AuthExceptionFilterAttribute : System.Web.Http.Filters.ExceptionFilterAttribute
	{
        public override void OnException(HttpActionExecutedContext context)
        {
            if (context.Exception is MissingCredentialsException)
                context.Response = new HttpResponseMessage(HttpStatusCode.Unauthorized);
            else if (context.Exception is InvalidCredentialsException)
                context.Response = new HttpResponseMessage(HttpStatusCode.Unauthorized);
            else if (context.Exception is ForbiddenException)
                context.Response = new HttpResponseMessage(HttpStatusCode.Forbidden);
        }
    }
}
