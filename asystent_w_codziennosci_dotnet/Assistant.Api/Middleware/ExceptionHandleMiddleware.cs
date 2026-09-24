using Assistant.Api.Contracts;
using AssistantLogic.IInternalServices;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;

namespace Assistant.Api.Middleware
{
    public class ExceptionHandleMiddleware
    {
        private readonly RequestDelegate _next;
        // Tworzymy jako singleton (¿yje ca³y czas)
        // IErrorService u¿ywa bazy danych. Prawdopodobnie jest zarejestrowany jako Scoped (¿yje tylko podczas jednego requesta)
        // Jak wstrzykn¹æ servis typu Scoped do naszego Middleware?
        // NIE ROBIMY TEGO W KONSTRUKTORZE!

        public ExceptionHandleMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, IErrorService errorService)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                errorService.LogError(ex.ToString());
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                context.Response.ContentType = "application/problem+json";
                await context.Response.WriteAsJsonAsync(new ErrorResponse("Wyst¹pi³ nieoczekiwany b³¹d serwera."));
            }
        }

    }

    // Helper ¿eby by³o czyœciej w Program.cs
    public static class ExceptionHandlingMiddlewareExtensions
    {
        public static IApplicationBuilder UseExceprionHandling(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ExceptionHandleMiddleware>();
        }
    }
}