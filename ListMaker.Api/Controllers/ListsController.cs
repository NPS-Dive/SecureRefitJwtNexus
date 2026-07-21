using ListMaker.Api.Features.Lists;
using ListMaker.Contracts.Lists;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace ListMaker.Api.Controllers;

/// <summary>
/// Provides secured list endpoints for generated person data.
/// </summary>
[ApiController]
[Route("api/lists")]
[Produces("application/json")]

public sealed class ListsController : ControllerBase
{
    private readonly IPersonListProvider _personListProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="ListsController"/> class.
    /// </summary>
    /// <param name="personListProvider">The stable person-list provider.</param>
    public ListsController ( IPersonListProvider personListProvider )
    {
        _personListProvider = personListProvider;
    }

    /// <summary>
    /// Gets the stable generated list of people.
    /// </summary>
    /// <returns>A stable list of generated person records.</returns>
    [Authorize]
    [HttpGet("generated")]
    [SwaggerOperation(
        Summary = "Get generated people",
        Description = "Returns 50 stable generated people. This endpoint requires JWT Bearer authentication.")]
    [SwaggerResponse(StatusCodes.Status200OK, "Generated people returned successfully.", typeof(IReadOnlyList<PersonListItemDto>))]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "Missing, invalid, or expired JWT token.")]
    public ActionResult<IReadOnlyList<PersonListItemDto>> GetGeneratedPeople ()
    {
        IReadOnlyList<PersonListItemDto> people = _personListProvider.GetGeneratedPeople();

        return Ok(people);
    }
}