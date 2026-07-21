using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;


namespace ListMaker.Api.Infrastructure.OpenApi;

/// <summary>
/// Adds JWT Bearer security metadata to Swagger operations that require authorization.
/// </summary>
public sealed class AuthorizeOperationFilter : IOperationFilter
    {
    /// <summary>
    /// Applies OpenAPI security metadata to controller actions protected by AuthorizeAttribute.
    /// </summary>
    /// <param name="operation">The generated OpenAPI operation.</param>
    /// <param name="context">The Swagger operation filter context.</param>
    public void Apply ( OpenApiOperation operation, OperationFilterContext context )
        {
        bool hasAuthorizeMetadata = context.ApiDescription.ActionDescriptor
            .EndpointMetadata
            .OfType<AuthorizeAttribute>()
            .Any();

        bool hasAllowAnonymousMetadata = context.ApiDescription.ActionDescriptor
            .EndpointMetadata
            .OfType<AllowAnonymousAttribute>()
            .Any();

        if (context.ApiDescription.ActionDescriptor is ControllerActionDescriptor controllerActionDescriptor)
            {
            bool methodHasAuthorize = controllerActionDescriptor.MethodInfo
                .GetCustomAttributes(inherit: true)
                .OfType<AuthorizeAttribute>()
                .Any();

            bool controllerHasAuthorize = controllerActionDescriptor.ControllerTypeInfo
                .GetCustomAttributes(inherit: true)
                .OfType<AuthorizeAttribute>()
                .Any();

            bool methodHasAllowAnonymous = controllerActionDescriptor.MethodInfo
                .GetCustomAttributes(inherit: true)
                .OfType<AllowAnonymousAttribute>()
                .Any();

            bool controllerHasAllowAnonymous = controllerActionDescriptor.ControllerTypeInfo
                .GetCustomAttributes(inherit: true)
                .OfType<AllowAnonymousAttribute>()
                .Any();

            hasAuthorizeMetadata =
                hasAuthorizeMetadata ||
                methodHasAuthorize ||
                controllerHasAuthorize;

            hasAllowAnonymousMetadata =
                hasAllowAnonymousMetadata ||
                methodHasAllowAnonymous ||
                controllerHasAllowAnonymous;
            }

        if (!hasAuthorizeMetadata || hasAllowAnonymousMetadata)
            {
            return;
            }

        operation.Responses ??= new OpenApiResponses();

        operation.Responses.TryAdd(
            "401",
            new OpenApiResponse
                {
                Description = "Missing, invalid, or expired JWT token."
                });

        operation.Security ??= new List<OpenApiSecurityRequirement>();

        operation.Security.Add(
            new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecuritySchemeReference("Bearer"),
                    new List<string>()
                }
            });
        }
    }
