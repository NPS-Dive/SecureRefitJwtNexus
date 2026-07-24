using ListMaker.Client.Authentication;
using ListMaker.Contracts.Authentication;
using ListReader.Api.Features.ListMakerIntegration.Configuration;
using ListReader.Api.Features.ListMakerIntegration.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Moq;

namespace ListReader.Api.UnitTests.ListMakerIntegration;

/// <summary>
/// Contains unit tests for <see cref="ListMakerAccessTokenCacheService"/>.
/// </summary>
[TestFixture]
public sealed class ListMakerAccessTokenCacheServiceTests
    {
    /// <summary>
    /// Verifies that when no cached token exists, the service authenticates
    /// against ListMaker.Api, caches the token, and returns it.
    /// </summary>
    [Test]
    public async Task GetAccessTokenAsync_WhenCacheIsEmpty_ShouldLoginCacheAndReturnToken ()
        {
        // Arrange
        MemoryCache memoryCache = new(new MemoryCacheOptions());

        Mock<IListMakerAuthApi> authApiMock = new(MockBehavior.Strict);
        authApiMock
            .Setup(api => api.LoginAsync(
                It.Is<LoginRequest>(request =>
                    request.Username == "@maker-service-user" &&
                    request.Password == "@maker-service-password"),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new LoginResponse
                {
                AccessToken = "downstream-token-1",
                TokenType = "Bearer",
                ExpiresAtUtc = DateTimeOffset.UtcNow.AddMinutes(10),
                ExpiresInSeconds = 600
                });

        ListMakerCredentialsOptions credentialsOptions = new()
            {
            Username = "@maker-service-user",
            Password = "@maker-service-password"
            };

        ListMakerAccessTokenCacheService sut = new(
            memoryCache,
            authApiMock.Object,
            Options.Create(credentialsOptions));

        // Act
        string firstToken = await sut.GetAccessTokenAsync(CancellationToken.None);
        string secondToken = await sut.GetAccessTokenAsync(CancellationToken.None);

        // Assert
        firstToken.Should().Be("downstream-token-1");
        secondToken.Should().Be("downstream-token-1");

        authApiMock.Verify(
            api => api.LoginAsync(It.IsAny<LoginRequest>(), It.IsAny<CancellationToken>()),
            Times.Once);
        }

    /// <summary>
    /// Verifies that a still-valid cached token is reused without a new login call.
    /// </summary>
    [Test]
    public async Task GetAccessTokenAsync_WhenCachedTokenIsStillValid_ShouldReturnCachedToken ()
        {
        // Arrange
        MemoryCache memoryCache = new(new MemoryCacheOptions());

        Mock<IListMakerAuthApi> authApiMock = new(MockBehavior.Strict);
        authApiMock
            .Setup(api => api.LoginAsync(It.IsAny<LoginRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new LoginResponse
                {
                AccessToken = "cached-valid-token",
                TokenType = "Bearer",
                ExpiresAtUtc = DateTimeOffset.UtcNow.AddMinutes(15),
                ExpiresInSeconds = 900
                });

        ListMakerCredentialsOptions credentialsOptions = new()
            {
            Username = "@maker-service-user",
            Password = "@maker-service-password"
            };

        ListMakerAccessTokenCacheService sut = new(
            memoryCache,
            authApiMock.Object,
            Options.Create(credentialsOptions));

        // Act
        string token1 = await sut.GetAccessTokenAsync(CancellationToken.None);
        string token2 = await sut.GetAccessTokenAsync(CancellationToken.None);

        // Assert
        token1.Should().Be("cached-valid-token");
        token2.Should().Be("cached-valid-token");

        authApiMock.Verify(
            api => api.LoginAsync(It.IsAny<LoginRequest>(), It.IsAny<CancellationToken>()),
            Times.Once);
        }

    /// <summary>
    /// Verifies that when the cached token is near expiration, the service
    /// performs a refresh and returns the new token.
    /// </summary>
    [Test]
    public async Task GetAccessTokenAsync_WhenCachedTokenIsNearExpiration_ShouldRefreshToken ()
        {
        // Arrange
        MemoryCache memoryCache = new(new MemoryCacheOptions());

        Queue<LoginResponse> loginResponses = new();
        loginResponses.Enqueue(new LoginResponse
            {
            AccessToken = "token-near-expiry",
            TokenType = "Bearer",
            ExpiresAtUtc = DateTimeOffset.UtcNow.AddSeconds(20),
            ExpiresInSeconds = 20
            });
        loginResponses.Enqueue(new LoginResponse
            {
            AccessToken = "token-refreshed",
            TokenType = "Bearer",
            ExpiresAtUtc = DateTimeOffset.UtcNow.AddMinutes(10),
            ExpiresInSeconds = 600
            });

        Mock<IListMakerAuthApi> authApiMock = new(MockBehavior.Strict);
        authApiMock
            .Setup(api => api.LoginAsync(It.IsAny<LoginRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => loginResponses.Dequeue());

        ListMakerCredentialsOptions credentialsOptions = new()
            {
            Username = "@maker-service-user",
            Password = "@maker-service-password"
            };

        ListMakerAccessTokenCacheService sut = new(
            memoryCache,
            authApiMock.Object,
            Options.Create(credentialsOptions));

        // Act
        string firstToken = await sut.GetAccessTokenAsync(CancellationToken.None);
        string secondToken = await sut.GetAccessTokenAsync(CancellationToken.None);

        // Assert
        firstToken.Should().Be("token-near-expiry");
        secondToken.Should().Be("token-refreshed");

        authApiMock.Verify(
            api => api.LoginAsync(It.IsAny<LoginRequest>(), It.IsAny<CancellationToken>()),
            Times.Exactly(2));
        }

    /// <summary>
    /// Verifies that missing downstream username configuration causes
    /// a clear invalid operation exception.
    /// </summary>
    [Test]
    public async Task GetAccessTokenAsync_WhenUsernameIsMissing_ShouldThrowInvalidOperationException ()
        {
        // Arrange
        MemoryCache memoryCache = new(new MemoryCacheOptions());

        Mock<IListMakerAuthApi> authApiMock = new(MockBehavior.Strict);

        ListMakerCredentialsOptions credentialsOptions = new()
            {
            Username = string.Empty,
            Password = "@maker-service-password"
            };

        ListMakerAccessTokenCacheService sut = new(
            memoryCache,
            authApiMock.Object,
            Options.Create(credentialsOptions));

        // Act
        Func<Task> act = async () => await sut.GetAccessTokenAsync(CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*ListMakerCredentials:Username*");
        }

    /// <summary>
    /// Verifies that missing downstream password configuration causes
    /// a clear invalid operation exception.
    /// </summary>
    [Test]
    public async Task GetAccessTokenAsync_WhenPasswordIsMissing_ShouldThrowInvalidOperationException ()
        {
        // Arrange
        MemoryCache memoryCache = new(new MemoryCacheOptions());

        Mock<IListMakerAuthApi> authApiMock = new(MockBehavior.Strict);

        ListMakerCredentialsOptions credentialsOptions = new()
            {
            Username = "@maker-service-user",
            Password = string.Empty
            };

        ListMakerAccessTokenCacheService sut = new(
            memoryCache,
            authApiMock.Object,
            Options.Create(credentialsOptions));

        // Act
        Func<Task> act = async () => await sut.GetAccessTokenAsync(CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*ListMakerCredentials:Password*");
        }

    /// <summary>
    /// Verifies that an empty downstream access token returned by ListMaker.Api
    /// is rejected with a clear exception.
    /// </summary>
    [Test]
    public async Task GetAccessTokenAsync_WhenLoginReturnsEmptyAccessToken_ShouldThrowInvalidOperationException ()
        {
        // Arrange
        MemoryCache memoryCache = new(new MemoryCacheOptions());

        Mock<IListMakerAuthApi> authApiMock = new(MockBehavior.Strict);
        authApiMock
            .Setup(api => api.LoginAsync(It.IsAny<LoginRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new LoginResponse
                {
                AccessToken = string.Empty,
                TokenType = "Bearer",
                ExpiresAtUtc = DateTimeOffset.UtcNow.AddMinutes(10),
                ExpiresInSeconds = 600
                });

        ListMakerCredentialsOptions credentialsOptions = new()
            {
            Username = "@maker-service-user",
            Password = "@maker-service-password"
            };

        ListMakerAccessTokenCacheService sut = new(
            memoryCache,
            authApiMock.Object,
            Options.Create(credentialsOptions));

        // Act
        Func<Task> act = async () => await sut.GetAccessTokenAsync(CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("ListMaker.Api returned an empty access token.");
        }
    }
