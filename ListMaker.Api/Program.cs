using System.Text;
using ListMaker.Api.Features.Authentication.Configuration;
using ListMaker.Api.Features.Authentication.Services;
using ListMaker.Api.Features.Lists;
using ListMaker.Api.Infrastructure.Authentication;
using ListMaker.Api.Infrastructure.OpenApi;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services
    .AddOptions<JwtOptions>()
    .Bind(builder.Configuration.GetSection(JwtOptions.SectionName))
    .Validate(
        options => !string.IsNullOrWhiteSpace(options.SigningKey),
        "JWT signing key is missing. Configure Jwt:SigningKey in configuration.")
    .Validate(
        options =>
            string.IsNullOrWhiteSpace(options.SigningKey)
            || options.SigningKey.Length >= JwtOptions.MinimumSigningKeyLength,
        $"JWT signing key must be at least {JwtOptions.MinimumSigningKeyLength} characters long.")
    .Validate(
        options => !string.IsNullOrWhiteSpace(options.Issuer),
        "JWT issuer is missing. Configure Jwt:Issuer in configuration.")
    .Validate(
        options => !string.IsNullOrWhiteSpace(options.Audience),
        "JWT audience is missing. Configure Jwt:Audience in configuration.")
    .Validate(
        options => options.ExpirationMinutes > 0,
        "JWT expiration must be greater than zero.")
    .ValidateOnStart();

builder.Services
    .AddOptions<StaticUserOptions>()
    .Bind(builder.Configuration.GetSection(StaticUserOptions.SectionName))
    .Validate(
        options => !string.IsNullOrWhiteSpace(options.Username),
        "Static username is missing. Configure StaticUser:Username in configuration.")
    .Validate(
        options => !string.IsNullOrWhiteSpace(options.Password),
        "Static password is missing. Configure StaticUser:Password in configuration.")
    .ValidateOnStart();

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer();

builder.Services
    .AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
    .Configure<IOptions<JwtOptions>, IWebHostEnvironment>(
        ( options, jwtOptionsAccessor, environment ) =>
        {
            JwtOptions jwtOptions = jwtOptionsAccessor.Value;

            options.RequireHttpsMetadata =
                !environment.IsEnvironment("Testing");

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

if (!app.Environment.IsEnvironment("Testing"))
    {
    app.UseHttpsRedirection();
    }

app.UseListMakerSwagger();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program
    {
    }
