using System.Globalization;
using ListMaker.Api.Features.Authentication.Configuration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace ListMaker.Api.IntegrationTests.Infrastructure;

/// <summary>
/// Provides a configured in-memory test host for <c>ListMaker.Api</c>.
/// </summary>
public sealed class ListMakerApiWebApplicationFactory : WebApplicationFactory<Program>
    {
    /// <summary>
    /// The static username accepted by the test host login endpoint.
    /// </summary>
    public const string TestUsername = "reader-user";

    /// <summary>
    /// The static password accepted by the test host login endpoint.
    /// </summary>
    public const string TestPassword = "reader-password";

    /// <summary>
    /// The JWT issuer used by the test host.
    /// </summary>
    public const string TestIssuer = "ListMaker.Api.IntegrationTests";

    /// <summary>
    /// The JWT audience used by the test host.
    /// </summary>
    public const string TestAudience = "ListMaker.Api.IntegrationTests.Clients";

    /// <summary>
    /// A test-only signing key for HMAC SHA-256.
    /// </summary>
    public const string TestSigningKey = "0123456789ABCDEF0123456789ABCDEF";

    /// <summary>
    /// The token expiration duration used by the test host.
    /// </summary>
    public const int TestExpirationMinutes = 30;

    /// <inheritdoc />
    protected override void ConfigureWebHost ( IWebHostBuilder builder )
        {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration(
            ( _, configurationBuilder ) =>
            {
                Dictionary<string, string?> testConfiguration = new()
                    {
                    [$"{StaticUserOptions.SectionName}:Username"] =
                        TestUsername,

                    [$"{StaticUserOptions.SectionName}:Password"] =
                        TestPassword,

                    [$"{JwtOptions.SectionName}:Issuer"] =
                        TestIssuer,

                    [$"{JwtOptions.SectionName}:Audience"] =
                        TestAudience,

                    [$"{JwtOptions.SectionName}:SigningKey"] =
                        TestSigningKey,

                    [$"{JwtOptions.SectionName}:ExpirationMinutes"] =
                        TestExpirationMinutes.ToString(CultureInfo.InvariantCulture)
                    };

                configurationBuilder.AddInMemoryCollection(testConfiguration);
            });
        }
    }
