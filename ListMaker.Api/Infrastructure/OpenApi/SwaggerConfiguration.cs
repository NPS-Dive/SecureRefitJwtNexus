using Microsoft.OpenApi;

namespace ListMaker.Api.Infrastructure.OpenApi;

/// <summary>
/// Provides Swagger/OpenAPI registration and middleware configuration.
/// </summary>
public static class SwaggerConfiguration
    {
    private const string BearerSchemeName = "Bearer";

    /// <summary>
    /// Registers Swagger generation with JWT Bearer authentication support.
    /// </summary>
    /// <param name="services">The application service collection.</param>
    /// <returns>The same service collection for chaining.</returns>
    public static IServiceCollection AddListMakerSwagger (
        this IServiceCollection services )
        {
        services.AddEndpointsApiExplorer();

        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc(
                "v1",
                new OpenApiInfo
                    {
                    Title = "ListMaker API",
                    Version = "v1",
                    Description =
                        "Authenticates clients and exposes stable generated person-list data."
                    });

            options.EnableAnnotations();

            // Defines the JWT Bearer authentication scheme.
            options.AddSecurityDefinition(
                BearerSchemeName,
                new OpenApiSecurityScheme
                    {
                    Name = "Authorization",
                    Description =
                        "Enter only the JWT access token. Swagger adds the Bearer prefix automatically.",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT"
                    });

            // The current Swashbuckle version expects a factory function
            // receiving the generated OpenAPI document.
            options.AddSecurityRequirement(document =>
                new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecuritySchemeReference(
                            BearerSchemeName,
                            document),
                        new List<string>()
                    }
                });

            // Keep the operation filter disabled temporarily because the
            // global security requirement is being used for diagnosis.
            // options.OperationFilter<AuthorizeOperationFilter>();
        });

        return services;
        }

    /// <summary>
    /// Enables the Swagger document and Swagger UI.
    /// </summary>
    /// <param name="app">The web application.</param>
    /// <returns>The same web application for chaining.</returns>
    public static WebApplication UseListMakerSwagger (
        this WebApplication app )
        {
        app.UseSwagger();

        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint(
                "/swagger/v1/swagger.json",
                "ListMaker API v1");

            options.DocumentTitle = "ListMaker API";

            // Retains the authorization value after refreshing Swagger UI.
            options.EnablePersistAuthorization();
        });

        return app;
        }
    }