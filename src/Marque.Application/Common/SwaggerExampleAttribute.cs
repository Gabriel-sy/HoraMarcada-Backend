using System;

namespace Marque.Application.Common;

/// <summary>
/// Specifies an example value for a property in the Swagger Schema.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
public class SwaggerExampleAttribute(string value) : Attribute
{
    public string Value { get; } = value;
}
