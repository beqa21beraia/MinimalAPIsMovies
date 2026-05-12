using Microsoft.OpenApi;
using MinimalAPIsMovies.DTOs;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Text.Json.Nodes;

namespace MinimalAPIsMovies.Utilities
{
    // Marker attributes to tag endpoints
    public class PaginationParametersAttribute : Attribute { }
    public class MoviesFilterParametersAttribute : Attribute { }

    public static class SwaggerExtensions
    {
        public static TBuilder AddMoviesFilterParameters<TBuilder>(this TBuilder builder)
            where TBuilder : IEndpointConventionBuilder
        {
            return builder.WithMetadata(new MoviesFilterParametersAttribute());
        }

        public static TBuilder AddPaginationParameters<TBuilder>(this TBuilder builder)
            where TBuilder : IEndpointConventionBuilder
        {
            return builder.WithMetadata(new PaginationParametersAttribute());
        }
    }

    public class SwaggerParametersFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var hasPagination = context.ApiDescription.ActionDescriptor.EndpointMetadata
                .OfType<PaginationParametersAttribute>().Any();
            var hasMoviesFilter = context.ApiDescription.ActionDescriptor.EndpointMetadata
                .OfType<MoviesFilterParametersAttribute>().Any();

            if (!hasPagination && !hasMoviesFilter) return;

            AddPaginationParams(operation);

            if (hasMoviesFilter)
                AddMoviesFilterParams(operation);
        }

        private void AddPaginationParams(OpenApiOperation operation)
        {
            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "Page",
                In = ParameterLocation.Query,
                Schema = new OpenApiSchema
                {
                    Type = JsonSchemaType.Integer,
                    Default = JsonValue.Create(PaginationDTO.pageInitialValue)
                }
            });

            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "RecordsPerPage",
                In = ParameterLocation.Query,
                Schema = new OpenApiSchema
                {
                    Type = JsonSchemaType.Integer,
                    Default = JsonValue.Create(PaginationDTO.recordsPerPageInitialValue)
                }
            });
        }

        private void AddMoviesFilterParams(OpenApiOperation operation)
        {
            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "Title",
                In = ParameterLocation.Query,
                Schema = new OpenApiSchema { Type = JsonSchemaType.String }
            });

            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "InTheaters",
                In = ParameterLocation.Query,
                Schema = new OpenApiSchema { Type = JsonSchemaType.Boolean }
            });

            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "GenreId",
                In = ParameterLocation.Query,
                Schema = new OpenApiSchema { Type = JsonSchemaType.Integer }
            });

            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "FutureReleases",
                In = ParameterLocation.Query,
                Schema = new OpenApiSchema { Type = JsonSchemaType.Boolean }
            });

            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "OrderByField",
                In = ParameterLocation.Query,
                Schema = new OpenApiSchema
                {
                    Type = JsonSchemaType.String,
                    Enum = new List<JsonNode>
                    {
                        JsonValue.Create("Title")!,
                        JsonValue.Create("ReleaseDate")!
                    }
                }
            });

            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "OrderByAscending",
                In = ParameterLocation.Query,
                Schema = new OpenApiSchema { Type = JsonSchemaType.Boolean }
            });
        }
    }
}