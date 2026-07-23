using ListMaker.Client.Lists;
using ListMaker.Contracts.Lists;
using Refit;

namespace ListReader.Api.Features.ListMakerIntegration.Services;

/// <summary>
/// Provides the application-facing gateway for calling protected
/// list endpoints on ListMaker.Api.
/// </summary>
public sealed class ListMakerGateway : IListMakerGateway
    {
    private readonly IListMakerAccessTokenCacheService _accessTokenCacheService;
    private readonly IListMakerListsApi _listMakerListsApi;
    private readonly ILogger<ListMakerGateway> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ListMakerGateway"/> class.
    /// </summary>
    /// <param name="accessTokenCacheService">
    /// The downstream access token cache service.
    /// </param>
    /// <param name="listMakerListsApi">
    /// The Refit client for protected list endpoints.
    /// </param>
    /// <param name="logger">
    /// The logger instance.
    /// </param>
    public ListMakerGateway (
        IListMakerAccessTokenCacheService accessTokenCacheService,
        IListMakerListsApi listMakerListsApi,
        ILogger<ListMakerGateway> logger )
        {
        _accessTokenCacheService = accessTokenCacheService;
        _listMakerListsApi = listMakerListsApi;
        _logger = logger;
        }

    /// <inheritdoc />
    public async Task<IReadOnlyList<PersonListItemDto>> GetGeneratedPeopleAsync (
        CancellationToken cancellationToken )
        {
        string accessToken = await _accessTokenCacheService.GetAccessTokenAsync(cancellationToken);

        try
            {
            return await _listMakerListsApi.GetGeneratedPeopleAsync(accessToken, cancellationToken);
            }
        catch (ApiException ex)
            {
            _logger.LogError(
                ex,
                "An error occurred while calling ListMaker.Api generated list endpoint. Status code: {StatusCode}",
                ex.StatusCode);

            throw;
            }
        }
    }
