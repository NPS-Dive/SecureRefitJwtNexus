using ListMaker.Client.Authentication;
using ListMaker.Client.Lists;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Refit;

namespace ListMaker.Client.DependencyInjection;

/// <summary>
/// Provides Dependency Injection registration methods for the ListMaker.Client library.
/// </summary>
/// <remarks>
/// This extension class registers Refit-based HTTP clients used to communicate with
/// ListMaker.Api.
/// 
/// The consuming application, such as ListReader.Api, only needs to call:
/// 
/// services.AddListMakerClient(configuration);
/// 
/// The client library remains stateless. It does not store credentials, cache JWT tokens,
/// or own authentication workflow state.
/// </remarks>
public static class ServiceCollectionExtensions
    {
    /// <summary>
    /// Registers ListMaker.Api Refit clients using configuration-bound options.
    /// </summary>
    /// <param name="services">
    /// The service collection used by the consuming application.
    /// </param>
    /// <param name="configuration">
    /// The application configuration containing the ListMakerClient section.
    /// </param>
    /// <returns>
    /// The same service collection, allowing chained DI registrations.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="services"/> or <paramref name="configuration"/> is null.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when required ListMakerClient configuration is missing or invalid.
    /// </exception>
    public static IServiceCollection AddListMakerClient (
        this IServiceCollection services,
        IConfiguration configuration )
        {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        ListMakerClientOptions options = BindAndValidateOptions(configuration);

        services.Configure<ListMakerClientOptions>(
            configuration.GetSection(ListMakerClientOptions.SectionName));

        services
            .AddRefitClient<IListMakerAuthApi>()
            .ConfigureHttpClient(client =>
            {
                client.BaseAddress = new Uri(options.BaseAddress);
                client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
            });

        services
            .AddRefitClient<IListMakerListsApi>()
            .ConfigureHttpClient(client =>
            {
                client.BaseAddress = new Uri(options.BaseAddress);
                client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
            });

        return services;
        }

    /// <summary>
    /// Binds and validates the ListMakerClient configuration section.
    /// </summary>
    /// <param name="configuration">
    /// The application configuration source.
    /// </param>
    /// <returns>
    /// A validated <see cref="ListMakerClientOptions"/> instance.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when required configuration values are missing or invalid.
    /// </exception>
    private static ListMakerClientOptions BindAndValidateOptions ( IConfiguration configuration )
    {
        IConfigurationSection section = configuration.GetSection(ListMakerClientOptions.SectionName);

        if (!section.Exists())
        {
            throw new InvalidOperationException(
                $"Missing required configuration section '{ListMakerClientOptions.SectionName}'.");
        }

        ListMakerClientOptions options = section.Get<ListMakerClientOptions>()
                                         ?? throw new InvalidOperationException(
                                             $"Unable to bind configuration section '{ListMakerClientOptions.SectionName}'.");

        if (string.IsNullOrWhiteSpace(options.BaseAddress))
        {
            throw new InvalidOperationException(
                $"Configuration value '{ListMakerClientOptions.SectionName}:{nameof(ListMakerClientOptions.BaseAddress)}' is required.");
        }

        if (!Uri.TryCreate(options.BaseAddress, UriKind.Absolute, out Uri? baseUri))
        {
            throw new InvalidOperationException(
                $"Configuration value '{ListMakerClientOptions.SectionName}:{nameof(ListMakerClientOptions.BaseAddress)}' must be a valid absolute URI.");
        }

        if (baseUri.Scheme != Uri.UriSchemeHttp && baseUri.Scheme != Uri.UriSchemeHttps)
        {
            throw new InvalidOperationException(
                $"Configuration value '{ListMakerClientOptions.SectionName}:{nameof(ListMakerClientOptions.BaseAddress)}' must use HTTP or HTTPS.");
        }

        if (options.TimeoutSeconds <= 0)
        {
            throw new InvalidOperationException(
                $"Configuration value '{ListMakerClientOptions.SectionName}:{nameof(ListMakerClientOptions.TimeoutSeconds)}' must be greater than zero.");
        }

        return options;
    }
    }
