using ListMaker.Contracts.Lists;

namespace ListReader.Api.Features.ListMakerIntegration.Services;

/// <summary>
/// Provides the application-facing gateway for calling ListMaker.Api.
/// </summary>
public interface IListMakerGateway
{
    /// <summary>
    /// Gets the generated person list from ListMaker.Api.
    /// </summary>
    /// <param name="cancellationToken">
    /// A token that allows the request to be cancelled.
    /// </param>
    /// <returns>
    /// The generated person list returned by ListMaker.Api.
    /// </returns>
    Task<IReadOnlyList<PersonListItemDto>> GetGeneratedPeopleAsync (
        CancellationToken cancellationToken );
}