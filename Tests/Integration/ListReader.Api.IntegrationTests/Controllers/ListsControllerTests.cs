using System.Net.Http.Headers;
using ListMaker.Contracts.Authentication;
using ListMaker.Contracts.Lists;
using ListReader.Api.IntegrationTests.Infrastructure;

namespace ListReader.Api.IntegrationTests.Controllers;

/// <summary>
/// Contains integration tests for list endpoints.
/// </summary>
[TestFixture]
public sealed class ListsControllerTests
    {
    private ListReaderApiWebApplicationFactory _factory = null!;
    private HttpClient _client = null!;

    /// <summary>
    /// Initializes the test host and HTTP client before each test.
    /// </summary>
    [SetUp]
    public void SetUp ()
        {
        _factory = new ListReaderApiWebApplicationFactory();
        _client = _factory.CreateClient();
        }

    /// <summary>
    /// Disposes test resources after each test.
    /// </summary>
    [TearDown]
    public void TearDown ()
        {
        _client.Dispose();
        _factory.Dispose();
        }

    /// <summary>
    /// Verifies that the protected endpoint rejects requests without a JWT token.
    /// </summary>
    [Test]
    public async Task GetGenerated_WithoutJwt_ShouldReturnUnauthorized ()
        {
        // Act
        HttpResponseMessage response = await _client.GetAsync("/api/lists/generated");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

    /// <summary>
    /// Verifies that the protected endpoint returns generated people for a valid JWT token.
    /// </summary>
    [Test]
    public async Task GetGenerated_WithValidJwt_ShouldReturnOkAndPeople ()
    {
        // Arrange
        string accessToken = await LoginAndGetAccessTokenAsync();

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", accessToken);

        // Act
        HttpResponseMessage response = await _client.GetAsync("/api/lists/generated");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        IReadOnlyList<PersonListItemDto>? payload =
            await response.Content.ReadFromJsonAsync<List<PersonListItemDto>>();

        payload.Should().NotBeNull();
        payload.Should().HaveCount(2);

        payload![0].Id.Should().Be(1);
        payload[0].Name.Should().Be("Sara");
        payload[0].Family.Should().Be("Johnson");
        payload[0].Age.Should().Be(29);
        payload[0].Gender.Should().Be("female");

        payload[1].Id.Should().Be(2);
        payload[1].Name.Should().Be("Omid");
        payload[1].Family.Should().Be("Rahimi");
        payload[1].Age.Should().Be(34);
        payload[1].Gender.Should().Be("male");
    }

    /// <summary>
    /// Verifies that downstream failures surface as server errors through the API pipeline.
    /// </summary>
    [Test]
    public async Task GetGenerated_WhenDownstreamThrows_ShouldReturnBadGateway ()
    {
        // Arrange
        _factory.FakeListsApi.ShouldThrowApiException = true;

        string accessToken = await LoginAndGetAccessTokenAsync();

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", accessToken);

        // Act
        HttpResponseMessage response = await _client.GetAsync("/api/lists/generated");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadGateway);
    }

    /// <summary>
    /// Authenticates against ListReader.Api and returns a valid JWT access token.
    /// </summary>
    /// <returns>
    /// The JWT access token issued by ListReader.Api.
    /// </returns>
    private async Task<string> LoginAndGetAccessTokenAsync ()
        {
        LoginRequest request = new()
            {
            Username = "reader@test",
            Password = "Reader@Test123!"
            };

        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/auth/login", request);
        response.EnsureSuccessStatusCode();

        LoginResponse? payload = await response.Content.ReadFromJsonAsync<LoginResponse>();

        payload.Should().NotBeNull();
        payload!.AccessToken.Should().NotBeNullOrWhiteSpace();

        return payload.AccessToken;
        }
    }
