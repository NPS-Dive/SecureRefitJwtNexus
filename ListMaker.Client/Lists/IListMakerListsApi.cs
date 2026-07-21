using ListMaker.Contracts.Lists;
using Refit;

namespace ListMaker.Client.Lists;

/// <summary>
/// Defines the typed HTTP client operations for list-related endpoints
/// exposed by ListMaker.Api.
/// </summary>
/// <remarks>
/// Refit generates the runtime HTTP client implementation for this interface.
/// 
/// This interface is intentionally focused only on list operations. Authentication
/// is handled separately by <c>IListMakerAuthApi</c>, and token management is owned
/// by the consuming application, such as ListReader.Api.
/// </remarks>
public interface IListMakerListsApi
{
    /// <summary>
    /// Gets the generated deterministic person list from ListMaker.Api.
    /// </summary>
    /// <param name="accessToken">
    /// The raw JWT access token issued by ListMaker.Api.
    /// 
    /// Important:
    /// Pass only the token value, without the "Bearer " prefix.
    /// Refit adds the "Bearer" authorization scheme automatically because of
    /// the <see cref="AuthorizeAttribute"/> applied to this parameter.
    /// </param>
    /// <param name="cancellationToken">
    /// A token that allows the caller to cancel the outgoing HTTP request.
    /// </param>
    /// <returns>
    /// A read-only list of generated person records returned by ListMaker.Api.
    /// </returns>
    /// <exception cref="ApiException">
    /// Thrown by Refit when ListMaker.Api returns a non-success HTTP status code,
    /// such as 401 Unauthorized, 403 Forbidden, or 500 Internal Server Error.
    /// </exception>
    [Get("/api/lists/generated")]
    Task<IReadOnlyList<PersonListItemDto>> GetGeneratedPeopleAsync (
        [Authorize("Bearer")] string accessToken,
        CancellationToken cancellationToken = default );
}