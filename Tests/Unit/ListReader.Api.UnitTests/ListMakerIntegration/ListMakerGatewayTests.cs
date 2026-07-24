using System.Net;
using System.Net.Http;
using ListMaker.Client.Lists;
using ListMaker.Contracts.Lists;
using ListReader.Api.Features.ListMakerIntegration.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Refit;

namespace ListReader.Api.UnitTests.ListMakerIntegration;

/// <summary>
/// Contains unit tests for <see cref="ListMakerGateway"/>.
/// </summary>
[TestFixture]
public sealed class ListMakerGatewayTests
    {
    /// <summary>
    /// Verifies that the gateway obtains a token from the cache service,
    /// calls the downstream generated list endpoint, and returns the result.
    /// </summary>
    [Test]
    public async Task GetGeneratedPeopleAsync_WhenCalled_ShouldUseTokenAndReturnPeople ()
        {
        // Arrange
        IReadOnlyList<PersonListItemDto> expectedPeople =
        [
            new PersonListItemDto
            {
                Id = 1,
                Name = "Leila",
                Family = "Akbari",
                Age = 31,
                Gender = "Female"
            },
            new PersonListItemDto
            {
                Id = 2,
                Name = "Navid",
                Family = "Ebrahimi",
                Age = 42,
                Gender = "Male"
            }
        ];

        Mock<IListMakerAccessTokenCacheService> tokenCacheServiceMock = new(MockBehavior.Strict);
        tokenCacheServiceMock
            .Setup(service => service.GetAccessTokenAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync("downstream-access-token");

        Mock<IListMakerListsApi> listsApiMock = new(MockBehavior.Strict);
        listsApiMock
            .Setup(api => api.GetGeneratedPeopleAsync(
                "downstream-access-token",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedPeople);

        Mock<ILogger<ListMakerGateway>> loggerMock = new();

        ListMakerGateway sut = new(
            tokenCacheServiceMock.Object,
            listsApiMock.Object,
            loggerMock.Object);

        // Act
        IReadOnlyList<PersonListItemDto> result =
            await sut.GetGeneratedPeopleAsync(CancellationToken.None);

        // Assert
        result.Should().BeEquivalentTo(expectedPeople);

        tokenCacheServiceMock.Verify(
            service => service.GetAccessTokenAsync(It.IsAny<CancellationToken>()),
            Times.Once);

        listsApiMock.Verify(
            api => api.GetGeneratedPeopleAsync("downstream-access-token", It.IsAny<CancellationToken>()),
            Times.Once);
        }

    /// <summary>
    /// Verifies that when the downstream Refit client throws an ApiException,
    /// the gateway logs and propagates the exception.
    /// </summary>
    [Test]
    public async Task GetGeneratedPeopleAsync_WhenDownstreamCallFails_ShouldRethrowApiException ()
        {
        // Arrange
        Mock<IListMakerAccessTokenCacheService> tokenCacheServiceMock = new(MockBehavior.Strict);
        tokenCacheServiceMock
            .Setup(service => service.GetAccessTokenAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync("downstream-access-token");

        HttpRequestMessage request = new(HttpMethod.Get, "https://localhost:7001/api/lists/generated");
        HttpResponseMessage response = new(HttpStatusCode.Unauthorized)
            {
            RequestMessage = request
            };

        ApiException apiException = await ApiException.Create(
            request,
            HttpMethod.Get,
            response,
            new RefitSettings());

        Mock<IListMakerListsApi> listsApiMock = new(MockBehavior.Strict);
        listsApiMock
            .Setup(api => api.GetGeneratedPeopleAsync(
                "downstream-access-token",
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(apiException);

        Mock<ILogger<ListMakerGateway>> loggerMock = new();

        ListMakerGateway sut = new(
            tokenCacheServiceMock.Object,
            listsApiMock.Object,
            loggerMock.Object);

        // Act
        Func<Task> act = async () => await sut.GetGeneratedPeopleAsync(CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ApiException>();

        tokenCacheServiceMock.Verify(
            service => service.GetAccessTokenAsync(It.IsAny<CancellationToken>()),
            Times.Once);

        listsApiMock.Verify(
            api => api.GetGeneratedPeopleAsync("downstream-access-token", It.IsAny<CancellationToken>()),
            Times.Once);
        }
    }
