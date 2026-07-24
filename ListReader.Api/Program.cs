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

builder.Services.Configure<JwtOptions>(
    builder.Configuration.GetSection(JwtOptions.SectionName));

builder.Services.Configure<StaticUserOptions>(
    builder.Configuration.GetSection(StaticUserOptions.SectionName));

builder.Services.Configure<ListMakerCredentialsOptions>(
    builder.Configuration.GetSection(ListMakerCredentialsOptions.SectionName));

JwtOptions readerJwtOptions = builder.Configuration
    .GetSection(JwtOptions.SectionName)
    .Get<JwtOptions>()
    ?? throw new InvalidOperationException(
        $"Missing configuration section '{JwtOptions.SectionName}'.");

if (string.IsNullOrWhiteSpace(readerJwtOptions.SecretKey))
    {
    throw new InvalidOperationException(
        $"Configuration value '{JwtOptions.SectionName}:{nameof(JwtOptions.SecretKey)}' is required.");
    }

if (readerJwtOptions.SecretKey.Length < 32)
    {
    throw new InvalidOperationException(
        $"Configuration value '{JwtOptions.SectionName}:{nameof(JwtOptions.SecretKey)}' must be at least 32 characters long.");
    }

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = true;
        options.SaveToken = false;

        options.TokenValidationParameters = new TokenValidationParameters
            {
            ValidateIssuer = true,
            ValidIssuer = readerJwtOptions.Issuer,

            ValidateAudience = true,
            ValidAudience = readerJwtOptions.Audience,

            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(readerJwtOptions.SecretKey)),

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

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program
{

}
