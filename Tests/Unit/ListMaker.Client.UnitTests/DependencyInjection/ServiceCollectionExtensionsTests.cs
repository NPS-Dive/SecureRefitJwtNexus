using ListMaker.Client.Authentication;
using ListMaker.Client.DependencyInjection;
using ListMaker.Client.Lists;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace ListMaker.Client.UnitTests.DependencyInjection;

/// <summary>
/// Contains unit tests for <see cref="ServiceCollectionExtensions"/>.
/// </summary>
[TestFixture]
public sealed class ServiceCollectionExtensionsTests
    {
    /// <summary>
    /// Verifies that AddListMakerClient throws an argument-null exception
    /// when the service collection is null.
    /// </summary>
    [Test]
    public void AddListMakerClient_WhenServicesIsNull_ShouldThrowArgumentNullException ()
        {
        // Arrange
        IServiceCollection? services = null;
        IConfiguration configuration = BuildConfiguration(
            baseAddress: "https://localhost:7001",
            timeoutSeconds: 30);

        // Act
        Action act = () => ServiceCollectionExtensions.AddListMakerClient(
            services!,
            configuration);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("services");
        }

    /// <summary>
    /// Verifies that AddListMakerClient throws an argument-null exception
    /// when the configuration object is null.
    /// </summary>
    [Test]
    public void AddListMakerClient_WhenConfigurationIsNull_ShouldThrowArgumentNullException ()
        {
        // Arrange
        IServiceCollection services = new ServiceCollection();
        IConfiguration? configuration = null;

        // Act
        Action act = () => services.AddListMakerClient(configuration!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("configuration");
        }

    /// <summary>
    /// Verifies that AddListMakerClient throws a clear invalid-operation exception
    /// when the required ListMakerClient configuration section is missing.
    /// </summary>
    [Test]
    public void AddListMakerClient_WhenConfigurationSectionIsMissing_ShouldThrowInvalidOperationException ()
        {
        // Arrange
        IServiceCollection services = new ServiceCollection();

        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>())
            .Build();

        // Act
        Action act = () => services.AddListMakerClient(configuration);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Missing required configuration section 'ListMakerClient'.");
        }

    /// <summary>
    /// Verifies that AddListMakerClient throws a clear invalid-operation exception
    /// when BaseAddress is missing or empty.
    /// </summary>
    [Test]
    public void AddListMakerClient_WhenBaseAddressIsMissing_ShouldThrowInvalidOperationException ()
        {
        // Arrange
        IServiceCollection services = new ServiceCollection();

        IConfiguration configuration = BuildConfiguration(
            baseAddress: string.Empty,
            timeoutSeconds: 30);

        // Act
        Action act = () => services.AddListMakerClient(configuration);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Configuration value 'ListMakerClient:BaseAddress' is required.");
        }

    /// <summary>
    /// Verifies that AddListMakerClient throws a clear invalid-operation exception
    /// when BaseAddress is not a valid absolute URI.
    /// </summary>
    [Test]
    public void AddListMakerClient_WhenBaseAddressIsNotAValidAbsoluteUri_ShouldThrowInvalidOperationException ()
        {
        // Arrange
        IServiceCollection services = new ServiceCollection();

        IConfiguration configuration = BuildConfiguration(
            baseAddress: "not-a-valid-uri",
            timeoutSeconds: 30);

        // Act
        Action act = () => services.AddListMakerClient(configuration);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Configuration value 'ListMakerClient:BaseAddress' must be a valid absolute URI.");
        }

    /// <summary>
    /// Verifies that AddListMakerClient throws a clear invalid-operation exception
    /// when BaseAddress uses a non-HTTP and non-HTTPS scheme.
    /// </summary>
    [Test]
    public void AddListMakerClient_WhenBaseAddressUsesUnsupportedScheme_ShouldThrowInvalidOperationException ()
        {
        // Arrange
        IServiceCollection services = new ServiceCollection();

        IConfiguration configuration = BuildConfiguration(
            baseAddress: "ftp://localhost:7001",
            timeoutSeconds: 30);

        // Act
        Action act = () => services.AddListMakerClient(configuration);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Configuration value 'ListMakerClient:BaseAddress' must use HTTP or HTTPS.");
        }

    /// <summary>
    /// Verifies that AddListMakerClient throws a clear invalid-operation exception
    /// when TimeoutSeconds is zero or negative.
    /// </summary>
    [Test]
    public void AddListMakerClient_WhenTimeoutSecondsIsNotGreaterThanZero_ShouldThrowInvalidOperationException ()
        {
        // Arrange
        IServiceCollection services = new ServiceCollection();

        IConfiguration configuration = BuildConfiguration(
            baseAddress: "https://localhost:7001",
            timeoutSeconds: 0);

        // Act
        Action act = () => services.AddListMakerClient(configuration);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Configuration value 'ListMakerClient:TimeoutSeconds' must be greater than zero.");
        }

    /// <summary>
    /// Verifies that AddListMakerClient returns the same service collection
    /// instance to support fluent DI registration chaining.
    /// </summary>
    [Test]
    public void AddListMakerClient_WithValidConfiguration_ShouldReturnSameServiceCollection ()
        {
        // Arrange
        IServiceCollection services = new ServiceCollection();
        IConfiguration configuration = BuildConfiguration(
            baseAddress: "https://localhost:7001",
            timeoutSeconds: 30);

        // Act
        IServiceCollection result = services.AddListMakerClient(configuration);

        // Assert
        result.Should().BeSameAs(services);
        }

    /// <summary>
    /// Verifies that AddListMakerClient registers the Refit-based authentication API client.
    /// </summary>
    [Test]
    public void AddListMakerClient_WithValidConfiguration_ShouldRegisterAuthApi ()
        {
        // Arrange
        IServiceCollection services = new ServiceCollection();
        IConfiguration configuration = BuildConfiguration(
            baseAddress: "https://localhost:7001",
            timeoutSeconds: 30);

        services.AddListMakerClient(configuration);
        IServiceProvider serviceProvider = services.BuildServiceProvider();

        // Act
        IListMakerAuthApi authApi = serviceProvider.GetRequiredService<IListMakerAuthApi>();

        // Assert
        authApi.Should().NotBeNull();
        }

    /// <summary>
    /// Verifies that AddListMakerClient registers the Refit-based lists API client.
    /// </summary>
    [Test]
    public void AddListMakerClient_WithValidConfiguration_ShouldRegisterListsApi ()
        {
        // Arrange
        IServiceCollection services = new ServiceCollection();
        IConfiguration configuration = BuildConfiguration(
            baseAddress: "https://localhost:7001",
            timeoutSeconds: 30);

        services.AddListMakerClient(configuration);
        IServiceProvider serviceProvider = services.BuildServiceProvider();

        // Act
        IListMakerListsApi listsApi = serviceProvider.GetRequiredService<IListMakerListsApi>();

        // Assert
        listsApi.Should().NotBeNull();
        }

    /// <summary>
    /// Verifies that AddListMakerClient registers and binds ListMakerClientOptions
    /// with the expected configuration values.
    /// </summary>
    [Test]
    public void AddListMakerClient_WithValidConfiguration_ShouldBindOptionsCorrectly ()
        {
        // Arrange
        IServiceCollection services = new ServiceCollection();
        IConfiguration configuration = BuildConfiguration(
            baseAddress: "https://localhost:7001",
            timeoutSeconds: 45);

        services.AddListMakerClient(configuration);
        IServiceProvider serviceProvider = services.BuildServiceProvider();

        // Act
        IOptions<ListMakerClientOptions> optionsAccessor =
            serviceProvider.GetRequiredService<IOptions<ListMakerClientOptions>>();

        ListMakerClientOptions options = optionsAccessor.Value;

        // Assert
        options.Should().NotBeNull();
        options.BaseAddress.Should().Be("https://localhost:7001");
        options.TimeoutSeconds.Should().Be(45);
        }

    /// <summary>
    /// Builds an in-memory configuration object for the ListMakerClient section.
    /// </summary>
    /// <param name="baseAddress">
    /// The ListMaker.Api base address value to place into configuration.
    /// </param>
    /// <param name="timeoutSeconds">
    /// The timeout value, in seconds, to place into configuration.
    /// </param>
    /// <returns>
    /// A configuration instance that mimics application settings.
    /// </returns>
    private static IConfiguration BuildConfiguration (
        string baseAddress,
        int timeoutSeconds )
        {
        Dictionary<string, string?> settings = new()
            {
            [$"{ListMakerClientOptions.SectionName}:{nameof(ListMakerClientOptions.BaseAddress)}"] = baseAddress,
            [$"{ListMakerClientOptions.SectionName}:{nameof(ListMakerClientOptions.TimeoutSeconds)}"] = timeoutSeconds.ToString()
            };

        return new ConfigurationBuilder()
            .AddInMemoryCollection(settings)
            .Build();
        }
    }
