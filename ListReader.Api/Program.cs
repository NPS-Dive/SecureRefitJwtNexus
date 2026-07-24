using System.Text;
using ListMaker.Client.DependencyInjection;
using ListReader.Api.Features.Authentication.Configuration;
using ListReader.Api.Features.Authentication.Services;
using ListReader.Api.Features.ListMakerIntegration.Configuration;
using ListReader.Api.Features.ListMakerIntegration.Services;
using ListReader.Api.Infrastructure.Authentication;
using ListReader.Api.Infrastructure.OpenApi;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerDocumentation();

/// <summary>
/// Registers and validates JWT configuration for ListReader.Api.
/// ValidateOnStart keeps production startup fail-fast while remaining
/// compatible with integration test host configuration overrides.
/// </summary>
builder.Services
    .AddOptions<JwtOptions>()
    .Bind(builder.Configuration.GetSection(JwtOptions.SectionName))
    .Validate(
        options => !string.IsNullOrWhiteSpace(options.Issuer),
        $"Configuration value '{JwtOptions.SectionName}:{nameof(JwtOptions.Issuer)}' is required.")
    .Validate(
        options => !string.IsNullOrWhiteSpace(options.Audience),
        $"Configuration value '{JwtOptions.SectionName}:{nameof(JwtOptions.Audience)}' is required.")
    .Validate(
        options => !string.IsNullOrWhiteSpace(options.SecretKey),
        $"Configuration value '{JwtOptions.SectionName}:{nameof(JwtOptions.SecretKey)}' is required.")
    .Validate(
        options => options.SecretKey.Length >= 32,
        $"Configuration value '{JwtOptions.SectionName}:{nameof(JwtOptions.SecretKey)}' must be at least 32 characters long.")
    .Validate(
        options => options.ExpirationMinutes > 0,
        $"Configuration value '{JwtOptions.SectionName}:{nameof(JwtOptions.ExpirationMinutes)}' must be greater than zero.")
    .ValidateOnStart();

/// <summary>
/// Registers and validates static demo credentials used to authenticate
/// external callers of ListReader.Api.
/// </summary>
builder.Services
    .AddOptions<StaticUserOptions>()
    .Bind(builder.Configuration.GetSection(StaticUserOptions.SectionName))
    .Validate(
        options => !string.IsNullOrWhiteSpace(options.Username),
        $"Configuration value '{StaticUserOptions.SectionName}:{nameof(StaticUserOptions.Username)}' is required.")
    .Validate(
        options => !string.IsNullOrWhiteSpace(options.Password),
        $"Configuration value '{StaticUserOptions.SectionName}:{nameof(StaticUserOptions.Password)}' is required.")
    .ValidateOnStart();

/// <summary>
/// Registers and validates downstream credentials used by ListReader.Api
/// to authenticate against ListMaker.Api.
/// </summary>
builder.Services
    .AddOptions<ListMakerCredentialsOptions>()
    .Bind(builder.Configuration.GetSection(ListMakerCredentialsOptions.SectionName))
    .Validate(
        options => !string.IsNullOrWhiteSpace(options.Username),
        $"Configuration value '{ListMakerCredentialsOptions.SectionName}:{nameof(ListMakerCredentialsOptions.Username)}' is required.")
    .Validate(
        options => !string.IsNullOrWhiteSpace(options.Password),
        $"Configuration value '{ListMakerCredentialsOptions.SectionName}:{nameof(ListMakerCredentialsOptions.Password)}' is required.")
    .ValidateOnStart();

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        JwtOptions jwtOptions = builder.Configuration
            .GetSection(JwtOptions.SectionName)
            .Get<JwtOptions>()
            ?? throw new InvalidOperationException(
                $"Missing configuration section '{JwtOptions.SectionName}'.");

        options.RequireHttpsMetadata = !builder.Environment.IsEnvironment("Testing");
        options.SaveToken = false;

        options.TokenValidationParameters = new TokenValidationParameters
            {
            ValidateIssuer = true,
            ValidIssuer = jwtOptions.Issuer,

            ValidateAudience = true,
            ValidAudience = jwtOptions.Audience,

            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtOptions.SecretKey)),

            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
            };
    });

builder.Services.AddAuthorization();

builder.Services.AddSingleton<IJwtTokenService, JwtTokenService>();

builder.Services.AddMemoryCache();

builder.Services.AddScoped<IListMakerGateway, ListMakerGateway>();
builder.Services.AddSingleton<IListMakerAccessTokenCacheService, ListMakerAccessTokenCacheService>();

builder.Services.AddListMakerClient(builder.Configuration);

WebApplication app = builder.Build();

if (app.Environment.IsDevelopment())
    {
    app.UseSwagger();
    app.UseSwaggerUI();
    }

/// <summary>
/// Avoid HTTPS redirection interference in automated in-memory integration tests.
/// </summary>
if (!app.Environment.IsEnvironment("Testing"))
    {
    app.UseHttpsRedirection();
    }

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program
    {
    }
