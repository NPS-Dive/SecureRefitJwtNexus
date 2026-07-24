using ListMaker.Client.Lists;
using ListMaker.Contracts.Lists;

namespace ListReader.Api.IntegrationTests.Infrastructure;

/// <summary>
/// Provides a fake downstream lists API for integration testing.
/// </summary>
public sealed class FakeListMakerListsApi : IListMakerListsApi
{
    /// <summary>
    /// Gets the in-memory people payload returned by the fake API.
    /// </summary>
    public IReadOnlyList<PersonListItemDto> People { get; init; } =
    [
        new PersonListItemDto
        {
            Id = 1,
            Name = "Sara",
            Family = "Johnson",
            Age = 29,
            Gender = "female"
        },
        new PersonListItemDto
        {
            Id = 2,
            Name = "Omid",
            Family = "Rahimi",
            Age = 34,
            Gender = "male"
        }
    ];

    /// <summary>
    /// Gets or sets a value indicating whether the fake API should throw.
    /// </summary>
    public bool ShouldThrowApiException { get; set; }

    /// <inheritdoc />
    public Task<IReadOnlyList<PersonListItemDto>> GetGeneratedPeopleAsync (
        string accessToken,
        CancellationToken cancellationToken = default )
    {
        if (ShouldThrowApiException)
        {
            throw new HttpRequestException(
                "Fake downstream ListMaker.Api failure.",
                null,
                HttpStatusCode.BadGateway);
        }

        return Task.FromResult(People);
    }
}