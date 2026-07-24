using ListMaker.Client.Authentication;
using ListMaker.Client.Lists;
using ListReader.Api;
using ListReader.Api.Features.Authentication.Configuration;
using ListReader.Api.Features.ListMakerIntegration.Configuration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ListReader.Api.IntegrationTests.Infrastructure;

/// <summary>
/// Provides a customized WebApplicationFactory for ListReader.Api integration tests.
/// </summary>
public sealed class ListReaderApiWebApplicationFactory
    : WebApplicationFactory<ListReaderApiAssemblyMarker>
    {
    /// <summary>
    /// Gets the fake downstream authentication API instance used by the test host.
    /// </summary>
    public FakeListMakerAuthApi FakeAuthApi { get; } = new();

    /// <summary>
    /// Gets the fake downstream lists API instance used by the test host.
    /// </summary>
    public FakeListMakerListsApi FakeListsApi { get; } = new();

    /// <inheritdoc />
    protected override void ConfigureWebHost ( IWebHostBuilder builder )
        {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration(( context, configurationBuilder ) =>
        {
            Dictionary<string, string?> settings = new()
                {
                [$"{JwtOptions.SectionName}:{nameof(JwtOptions.Issuer)}"] = "ListReader.Api.TestIssuer",
                [$"{JwtOptions.SectionName}:{nameof(JwtOptions.Audience)}"] = "ListReader.Api.TestAudience",
                [$"{JwtOptions.SectionName}:{nameof(JwtOptions.SecretKey)}"] = "ThisIsAStrongIntegrationTestSecretKey1234567890",
                [$"{JwtOptions.SectionName}:{nameof(JwtOptions.ExpirationMinutes)}"] = "60",

                [$"{StaticUserOptions.SectionName}:{nameof(StaticUserOptions.Username)}"] = "reader@test",
                [$"{StaticUserOptions.SectionName}:{nameof(StaticUserOptions.Password)}"] = "Reader@Test123!",

                [$"{ListMakerCredentialsOptions.SectionName}:{nameof(ListMakerCredentialsOptions.Username)}"] = "maker@test",
                [$"{ListMakerCredentialsOptions.SectionName}:{nameof(ListMakerCredentialsOptions.Password)}"] = "Maker@Test123!",

                ["ListMakerClient:BaseAddress"] = "https://fake-listmaker",
                ["ListMakerClient:TimeoutSeconds"] = "30"
                };

            configurationBuilder.AddInMemoryCollection(settings);
        });

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<IListMakerAuthApi>();
            services.RemoveAll<IListMakerListsApi>();

            services.AddSingleton<IListMakerAuthApi>(FakeAuthApi);
            services.AddSingleton<IListMakerListsApi>(FakeListsApi);
        });
        }
    }
