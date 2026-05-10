using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection.Metadata;

namespace MinimalAPIsMovies.Swagger
{
    public class AuthorizationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var isAuthorized = context.ApiDescription.ActionDescriptor.EndpointMetadata
            .OfType<AuthorizeAttribute>().Any();

            if (!isAuthorized)
            {
                operation.Security = new List<OpenApiSecurityRequirement>();
            }
        }
    }
}
