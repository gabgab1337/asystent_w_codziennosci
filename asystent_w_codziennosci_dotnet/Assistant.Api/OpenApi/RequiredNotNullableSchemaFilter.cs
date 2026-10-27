using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Assistant.Api.Contracts;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Assistant.Api.OpenApi
{
    /// <summary>
    /// Marks non-nullable reference-type DTO properties as required in OpenAPI.
    /// OpenAPI nullability alone is insufficient for generators that use the required list.
    /// </summary>
    internal sealed class RequiredNotNullableSchemaFilter : ISchemaFilter
    {
        private static readonly string ContractsNamespace = typeof(UserDto).Namespace!;

        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
        {
            if (context.Type.Namespace != ContractsNamespace)
            {
                return;
            }

            var nullabilityContext = new NullabilityInfoContext();
            foreach (PropertyInfo property in context.Type.GetProperties(
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly))
            {
                if (property.PropertyType.IsValueType
                    || nullabilityContext.Create(property).ReadState != NullabilityState.NotNull)
                {
                    continue;
                }

                string expectedName = property.GetCustomAttribute<JsonPropertyNameAttribute>()?.Name
                    ?? JsonNamingPolicy.CamelCase.ConvertName(property.Name);
                MarkRequired(schema, expectedName, property.Name);
            }
        }

        private static bool MarkRequired(
            OpenApiSchema schema,
            string expectedName,
            string clrPropertyName)
        {
            string? schemaPropertyName = schema.Properties.Keys.FirstOrDefault(name =>
                string.Equals(name, expectedName, StringComparison.Ordinal)
                || string.Equals(name, clrPropertyName, StringComparison.OrdinalIgnoreCase));

            if (schemaPropertyName != null)
            {
                schema.Required.Add(schemaPropertyName);
                return true;
            }

            foreach (OpenApiSchema nestedSchema in schema.AllOf)
            {
                if (MarkRequired(nestedSchema, expectedName, clrPropertyName))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
