using Microsoft.OpenApi;

namespace ListReader.Api.Infrastructure.OpenApi;

/// <summary>
/// Provides Swagger/OpenAPI configuration for ListReader.Api.
/// </summary>
public static class SwaggerConfiguration
{
    /// <summary>
    /// Registers Swagger/OpenAPI services.
    /// </summary>
    /// <param name="services">
    /// The service collection.
    /// </param>
    /// <returns>
    /// The same service collection for chaining.
    /// </returns>
    public static IServiceCollection AddSwaggerDocumentation ( this IServiceCollection services )
    {
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "ListReader.Api",
                Version = "v1",
                Description = "Reads generated person data by securely calling ListMaker.Api using Refit and JWT."
            });

            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Enter your JWT token only. Do not type Bearer manually."
            });

            options.AddSecurityRequirement(openApiDocument => new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecuritySchemeReference("Bearer", openApiDocument, null),
                    new List<string>()
                }
            });
        });

        return services;
    }
}