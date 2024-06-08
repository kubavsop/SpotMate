using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace SpotMate.Web.Configurations;

internal class SwaggerAuthOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        IEnumerable<CustomAttributeData> attributes = context.MethodInfo.CustomAttributes;
        attributes = attributes.Concat(context.MethodInfo.DeclaringType?.CustomAttributes ?? Enumerable.Empty<CustomAttributeData>());

        bool thereIsAuthorize = attributes.Any(attribute => attribute.AttributeType.Name.Contains(nameof(AuthorizeAttribute)));
        if (thereIsAuthorize)
        {
            operation.Security.Add(
                new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        new string[] {}
                    }
                }
            );
        }
    }
}