using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Linq;
using System.Reflection;
using HoraMarcada.Application.Common;

namespace HoraMarcada.Api.Filters;

/// <summary>
/// Swagger schema filter to extract property example values from <see cref="SwaggerExampleAttribute"/> and map them correctly.
/// </summary>
public class SwaggerExampleSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (schema.Properties == null) return;

        foreach (var property in schema.Properties)
        {
            var memberInfo = context.Type
                .GetMember(property.Key, BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase)
                .FirstOrDefault();

            if (memberInfo == null) continue;

            var exampleAttr = memberInfo.GetCustomAttribute<SwaggerExampleAttribute>();
            if (exampleAttr == null) continue;

            var value = exampleAttr.Value;
            IOpenApiAny apiValue = property.Value.Type switch
            {
                "integer" => int.TryParse(value, out var intVal) ? new OpenApiInteger(intVal) : new OpenApiString(value),
                "number" => double.TryParse(value, out var doubleVal) ? new OpenApiDouble(doubleVal) : new OpenApiString(value),
                "boolean" => bool.TryParse(value, out var boolVal) ? new OpenApiBoolean(boolVal) : new OpenApiString(value),
                _ => new OpenApiString(value)
            };

            property.Value.Example = apiValue;
        }
    }
}
