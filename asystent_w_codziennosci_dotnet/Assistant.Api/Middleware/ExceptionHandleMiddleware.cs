using Assistant.Api.Contracts;
using AssistantLogic.IInternalServices;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Assistant.Api.Middleware
{
    public class ExceptionHandleMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandleMiddleware> _logger;

        public ExceptionHandleMiddleware(RequestDelegate next, ILogger<ExceptionHandleMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, IErrorService errorService)
        {
            try 
            { 
                await _next(context); 
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Nieobsłużony wyjątek");
                try
                {
                    errorService.LogError(ex.ToString());
                }
                catch (Exception dbEx)
                {
                    _logger.LogError(dbEx, "Nieobsłużony wyjątek");
                }
                if (!context.Response.HasStarted)
                {
                    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                    await context.Response.WriteAsJsonAsync(
                        new ErrorResponse("Wystąpił nieoczekiwany błąd serwera.") { Code = "unexpected" },
                        options: null,
                        contentType: "application/problem+json");
                }
            }
        }

    }

    public static class ExceptionHandlingMiddlewareExtensions
    {
        public static IApplicationBuilder UseExceptionHandling(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ExceptionHandleMiddleware>();
        }
    }
}