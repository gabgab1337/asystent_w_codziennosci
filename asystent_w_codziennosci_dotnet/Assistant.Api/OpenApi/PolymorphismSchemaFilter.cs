using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Assistant.Api.OpenApi
{
    /// <summary>
    /// Adds the discriminator directly to every inline oneOf emitted by Swashbuckle.
    /// Some client generators do not follow the discriminator from the referenced base schema.
    /// </summary>
    internal sealed class PolymorphismSchemaFilter : ISchemaFilter
    {
        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
        {
            PolymorphismSchema.ApplyDiscriminators(schema);
        }
    }

    /// <summary>
    /// Covers top-level request and response schemas, which are created after schema filters run.
    /// </summary>
    internal sealed class PolymorphismOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            if (operation.RequestBody?.Content != null)
            {
                foreach (OpenApiMediaType mediaType in operation.RequestBody.Content.Values)
                {
                    PolymorphismSchema.ApplyDiscriminators(mediaType.Schema);
                }
            }

            foreach (OpenApiResponse response in operation.Responses.Values)
            {
                foreach (OpenApiMediaType mediaType in response.Content.Values)
                {
                    PolymorphismSchema.ApplyDiscriminators(mediaType.Schema);
                }
            }

            foreach (OpenApiParameter parameter in operation.Parameters)
            {
                PolymorphismSchema.ApplyDiscriminators(parameter.Schema);
            }
        }
    }

    internal static class PolymorphismSchema
    {
        private const string DiscriminatorPropertyName = "kind";

        private static readonly IReadOnlyList<PolymorphicFamily> Families =
            new List<PolymorphicFamily>
            {
                new(
                    "TimeTriggerDto",
                    "WeatherTriggerDto"),
                new(
                    "CreateTimeTriggerRequest",
                    "CreateWeatherTriggerRequest"),
                new(
                    "UpdateTimeTriggerRequest",
                    "UpdateWeatherTriggerRequest")
            };

        public static void ApplyDiscriminators(OpenApiSchema? schema)
        {
            if (schema == null)
            {
                return;
            }

            PolymorphicFamily? family = Families.SingleOrDefault(candidate =>
                candidate.Matches(schema.OneOf));
            if (family != null)
            {
                schema.Discriminator = family.CreateDiscriminator();
            }

            ApplyDiscriminators(schema.Items);

            foreach (OpenApiSchema property in schema.Properties.Values)
            {
                ApplyDiscriminators(property);
            }

            foreach (OpenApiSchema nestedSchema in schema.AllOf)
            {
                ApplyDiscriminators(nestedSchema);
            }

            foreach (OpenApiSchema nestedSchema in schema.AnyOf)
            {
                ApplyDiscriminators(nestedSchema);
            }

            foreach (OpenApiSchema nestedSchema in schema.OneOf)
            {
                ApplyDiscriminators(nestedSchema);
            }
        }

        private sealed class PolymorphicFamily
        {
            private readonly string firstSchemaId;
            private readonly string secondSchemaId;

            public PolymorphicFamily(string firstSchemaId, string secondSchemaId)
            {
                this.firstSchemaId = firstSchemaId;
                this.secondSchemaId = secondSchemaId;
            }

            public bool Matches(IList<OpenApiSchema> schemas)
            {
                if (schemas.Count != 2)
                {
                    return false;
                }

                HashSet<string?> schemaIds = schemas
                    .Select(schema => schema.Reference?.Id)
                    .ToHashSet(StringComparer.Ordinal);

                return schemaIds.SetEquals(new[] { firstSchemaId, secondSchemaId });
            }

            public OpenApiDiscriminator CreateDiscriminator()
            {
                return new OpenApiDiscriminator
                {
                    PropertyName = DiscriminatorPropertyName,
                    Mapping = new Dictionary<string, string>
                    {
                        ["time"] = $"#/components/schemas/{firstSchemaId}",
                        ["weather"] = $"#/components/schemas/{secondSchemaId}"
                    }
                };
            }
        }
    }
}
