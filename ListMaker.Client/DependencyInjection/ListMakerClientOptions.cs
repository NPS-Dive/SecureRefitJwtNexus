namespace ListMaker.Client.DependencyInjection;

/// <summary>
/// Represents configuration settings used by the ListMaker.Client library
/// when registering Refit-based HTTP clients.
/// </summary>
/// <remarks>
/// This options class is intended to be bound from application configuration
/// by the consuming application, such as ListReader.Api.
/// 
/// Example configuration:
/// 
/// {
///   "ListMakerClient": {
///     "BaseAddress": "https://localhost:7001",
///     "TimeoutSeconds": 30
///   }
/// }
/// 
/// The client library only uses these values to configure outbound HTTP
/// communication. It does not own authentication credentials or token caching.
/// </remarks>
public sealed class ListMakerClientOptions
    {
    /// <summary>
    /// The default configuration section name used to bind this options object.
    /// </summary>
    /// <remarks>
    /// A consuming application can use this section name in appsettings.json:
    /// 
    /// {
    ///   "ListMakerClient": {
    ///     "BaseAddress": "https://localhost:7001",
    ///     "TimeoutSeconds": 30
    ///   }
    /// }
    /// </remarks>
    public const string SectionName = "ListMakerClient";

    /// <summary>
    /// The base address of the ListMaker.Api service.
    /// </summary>
    /// <remarks>
    /// This value should point to the service root, not to a specific API route.
    /// 
    /// Correct:
    /// https://localhost:7001
    /// 
    /// Incorrect:
    /// https://localhost:7001/api
    /// 
    /// Refit interfaces already define route paths such as:
    /// /api/auth/login
    /// /api/lists/generated
    /// 
    /// Therefore, the base address should only contain the protocol, host,
    /// and optional port.
    /// </remarks>
    public string BaseAddress { get; init; } = string.Empty;

    /// <summary>
    /// The timeout, in seconds, for outgoing HTTP requests to ListMaker.Api.
    /// </summary>
    /// <remarks>
    /// The default value is 30 seconds.
    /// 
    /// This is intentionally configurable because local development,
    /// containerized environments, staging, and production may need different
    /// timeout policies.
    /// 
    /// This timeout is a basic HTTP client timeout. Later, if we add advanced
    /// resilience policies, such as retries or circuit breakers, those policies
    /// should be configured separately.
    /// </remarks>
    public int TimeoutSeconds { get; init; } = 30;
    }
