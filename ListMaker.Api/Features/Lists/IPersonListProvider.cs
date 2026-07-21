using ListMaker.Contracts.Lists;

namespace ListMaker.Api.Features.Lists;

/// <summary>
/// Defines behavior for providing person-list data.
/// </summary>
public interface IPersonListProvider
{
    /// <summary>
    /// Gets a stable list of generated people.
    /// </summary>
    /// <returns>
    /// A read-only collection of generated person records.
    /// </returns>
    IReadOnlyList<PersonListItemDto> GetGeneratedPeople ();
}