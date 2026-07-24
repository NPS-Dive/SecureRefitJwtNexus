using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using ListMaker.Api.IntegrationTests.Infrastructure;
using ListMaker.Contracts.Authentication;
using ListMaker.Contracts.Lists;

namespace ListMaker.Api.IntegrationTests.Lists;

/// <summary>
/// Contains integration tests for generated people endpoints of <c>ListMaker.Api</c>.
/// </summary>
[TestFixture]
public sealed class GeneratedPeopleEndpointTests
    {
    private ListMakerApiWebApplicationFactory _factory = null!;
    private HttpClient _httpClient = null!;

    /// <summary>
    /// Creates a fresh test host and HTTP client before each test.
    /// </summary>
    [SetUp]
    public void SetUp ()
        {
        _factory = new ListMakerApiWebApplicationFactory();
        _httpClient = _factory.CreateClient();
        }

    /// <summary>
    /// Disposes the HTTP client and test host after each test.
    /// </summary>
    [TearDown]
    public void TearDown ()
        {
        _httpClient?.Dispose();
        _factory?.Dispose();
        }

    /// <summary>
    /// Verifies that the generated people endpoint rejects unauthenticated callers.
    /// </summary>
    [Test]
    public async Task GetGeneratedPeople_WithoutBearerToken_ShouldReturnUnauthorized ()
        {
        // Act
        HttpResponseMessage response = await _httpClient.GetAsync("/api/lists/generated");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

    /// <summary>
    /// Verifies that the generated people endpoint rejects malformed bearer tokens.
    /// </summary>
    [Test]
    public async Task GetGeneratedPeople_WithInvalidBearerToken_ShouldReturnUnauthorized ()
        {
        // Arrange
        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", "invalid-token-value");

        // Act
        HttpResponseMessage response = await _httpClient.GetAsync("/api/lists/generated");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

    /// <summary>
    /// Verifies that an authenticated caller can retrieve the stable generated people list.
    /// </summary>
    [Test]
    public async Task GetGeneratedPeople_WithValidBearerToken_ShouldReturnOkWithFiftyPeople ()
        {
        // Arrange
        string accessToken = await LoginAndGetAccessTokenAsync();

        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", accessToken);

        // Act
        HttpResponseMessage response = await _httpClient.GetAsync("/api/lists/generated");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        IReadOnlyList<PersonListItemDto>? people =
            await response.Content.ReadFromJsonAsync<IReadOnlyList<PersonListItemDto>>();

        people.Should().NotBeNull();
        people!.Should().HaveCount(50);

        people[0].Should().BeEquivalentTo(
            new PersonListItemDto
                {
                Id = 1,
                Name = "Leila",
                Family = "Akbari",
                Age = 38,
                Gender = "Male"
                });

        people[24].Should().BeEquivalentTo(
            new PersonListItemDto
                {
                Id = 25,
                Name = "Navid",
                Family = "Ebrahimi",
                Age = 44,
                Gender = "Female"
                });

        people[49].Should().BeEquivalentTo(
            new PersonListItemDto
                {
                Id = 50,
                Name = "Mina",
                Family = "Ghasemi",
                Age = 52,
                Gender = "Female"
                });
        }

    /// <summary>
    /// Logs in against the in-memory test server and returns the issued access token.
    /// </summary>
    /// <returns>The issued JWT access token.</returns>
    private async Task<string> LoginAndGetAccessTokenAsync ()
        {
        var loginRequest = new LoginRequest
            {
            Username = ListMakerApiWebApplicationFactory.TestUsername,
            Password = ListMakerApiWebApplicationFactory.TestPassword
            };

        HttpResponseMessage loginResponse = await _httpClient.PostAsJsonAsync(
            "/api/auth/login",
            loginRequest);

        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        LoginResponse? loginContent =
            await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();

        loginContent.Should().NotBeNull();
        loginContent!.AccessToken.Should().NotBeNullOrWhiteSpace();

        return loginContent.AccessToken;
        }
    }
