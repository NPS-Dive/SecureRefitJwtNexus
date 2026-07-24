using System.Text;
using ListMaker.Api.Features.Authentication.Configuration;
using ListMaker.Api.Features.Authentication.Services;
using ListMaker.Api.Features.Lists;
using ListMaker.Api.Infrastructure.Authentication;
using ListMaker.Api.Infrastructure.OpenApi;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.Configure<JwtOptions>(
    builder.Configuration.GetSection(JwtOptions.SectionName));

builder.Services.Configure<StaticUserOptions>(
    builder.Configuration.GetSection(StaticUserOptions.SectionName));

JwtOptions jwtOptions = builder.Configuration
    .GetSection(JwtOptions.SectionName)
    .Get<JwtOptions>()
    ?? new JwtOptions();

if (string.IsNullOrWhiteSpace(jwtOptions.SigningKey))
    {
    throw new InvalidOperationException(
        "JWT signing key is missing. Configure Jwt:SigningKey in appsettings.Development.json or another configuration source.");
    }

if (string.IsNullOrWhiteSpace(jwtOptions.Issuer))
    {
    throw new InvalidOperationException(
        "JWT issuer is missing. Configure Jwt:Issuer in configuration.");
    }

if (string.IsNullOrWhiteSpace(jwtOptions.Audience))
    {
    throw new InvalidOperationException(
        "JWT audience is missing. Configure Jwt:Audience in configuration.");
    }

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = true;
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
            {
            ValidateIssuer = true,
            ValidIssuer = jwtOptions.Issuer,

            ValidateAudience = true,
            ValidAudience = jwtOptions.Audience,

            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtOptions.SigningKey)),

            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
            };
    });

builder.Services.AddAuthorization();

builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddSingleton<IPersonListProvider, StablePersonListProvider>();

builder.Services.AddListMakerSwagger();

var app = builder.Build();

app.UseHttpsRedirection();

app.UseListMakerSwagger();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();


public partial class Program
{

}