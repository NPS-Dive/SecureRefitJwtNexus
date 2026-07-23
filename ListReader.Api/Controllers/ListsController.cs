using ListMaker.Contracts.Lists;
using ListReader.Api.Features.ListMakerIntegration.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ListReader.Api.Controllers;

/// <summary>
/// Exposes list-related endpoints for ListReader.Api.
/// </summary>
/// <remarks>
/// This controller requires a valid ListReader.Api JWT token from the caller.
/// It then uses the downstream ListMaker integration layer to obtain the
/// generated list from ListMaker.Api and relays the result back to the caller.
/// </remarks>
[ApiController]
[Route("api/[controller]")]
public sealed class ListsController : ControllerBase
    {
    private readonly IListMakerGateway _listMakerGateway;

    /// <summary>
    /// Initializes a new instance of the <see cref="ListsController"/> class.
    /// </summary>
    /// <param name="listMakerGateway">
    /// The gateway used to authenticate with and call ListMaker.Api.
    /// </param>
    public ListsController ( IListMakerGateway listMakerGateway )
        {
        _listMakerGateway = listMakerGateway;
        }

    /// <summary>
    /// Gets the generated person list by calling ListMaker.Api.
    /// </summary>
    /// <param name="cancellationToken">
    /// A token that allows the request to be cancelled.
    /// </param>
    /// <returns>
    /// The generated list retrieved from ListMaker.Api.
    /// </returns>
    [Authorize]
    [HttpGet("generated")]
    [ProducesResponseType(typeof(IReadOnlyList<PersonListItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status502BadGateway)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IReadOnlyList<PersonListItemDto>>> GetGeneratedAsync (
        CancellationToken cancellationToken )
        {
        IReadOnlyList<PersonListItemDto> people =
            await _listMakerGateway.GetGeneratedPeopleAsync(cancellationToken);

        return Ok(people);
        }
    }
