using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Assistant.Api.OpenApi
{
    /// <summary>
    /// Prevents ApiController conventions from attaching ProblemDetails to responses explicitly
    /// documented as bodyless, such as an invalid one-time file ticket.
    /// </summary>
    internal sealed class EmptyResponseOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            IEnumerable<ProducesResponseTypeAttribute> emptyResponseAttributes = context.MethodInfo
                .GetCustomAttributes(inherit: true)
                .OfType<ProducesResponseTypeAttribute>()
                .Where(attribute => attribute.Type == typeof(void));

            foreach (ProducesResponseTypeAttribute attribute in emptyResponseAttributes)
            {
                string statusCode = attribute.StatusCode.ToString();
                if (operation.Responses.TryGetValue(statusCode, out OpenApiResponse? response))
                {
                    response.Content.Clear();
                }
            }
        }
    }
}
