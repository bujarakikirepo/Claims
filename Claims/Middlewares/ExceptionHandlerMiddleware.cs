using Application.Exceptions;
using System.Net;

namespace Claims.Middlewares
{
    public class ExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _next;

        public ExceptionHandlerMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (BadRequestException customException)
            {
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                context.Response.ContentType = "text/plain";
                await context.Response.WriteAsync(customException.Message);
            }
            catch (EntityNotFoundException entityNotFoundException)
            {
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                context.Response.ContentType = "text/plain";
                await context.Response.WriteAsync(entityNotFoundException.Message);
            }
        }
    }

}
