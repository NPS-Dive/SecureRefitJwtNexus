namespace ListMaker.Contracts.Lists;

/// <summary>
/// Represents one person item returned by the ListMaker API.
/// </summary>
/// <remarks>
/// <c>ListMaker.Api</c> will generate a stable seeded list of 50 records.
/// <c>ListReader.Api</c> will call <c>ListMaker.Api</c> and return these items
/// to its own caller.
///
/// Gender is deliberately represented as a string because this is a demo and
/// you accepted that decision.
/// </remarks>
public sealed record PersonListItemDto
    {
    /// <summary>
    /// Gets the stable identifier of the generated person.
    /// </summary>
    /// <remarks>
    /// This is useful for stable seeded data, testing, Swagger inspection,
    /// and client-side rendering.
    /// </remarks>
    /// <example>1</example>
    public required int Id { get; set; }

    /// <summary>
    /// Gets the person's given name.
    /// </summary>
    /// <remarks>
    /// Expected maximum logical length: 50 characters.
    /// Actual length enforcement will be handled by the producing API if needed.
    /// </remarks>
    /// <example>Alex</example>
    public required string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets the person's family name.
    /// </summary>
    /// <remarks>
    /// Expected maximum logical length: 50 characters.
    /// Actual length enforcement will be handled by the producing API if needed.
    /// </remarks>
    /// <example>Johnson</example>
    public required string Family { get; set; } = string.Empty;

    /// <summary>
    /// Gets the person's age.
    /// </summary>
    /// <remarks>
    /// The seeded data generator will produce ages between 18 and 65 inclusive.
    /// </remarks>
    /// <example>34</example>
    public required int Age { get; init; }

    /// <summary>
    /// Gets the person's gender as a string.
    /// </summary>
    /// <remarks>
    /// Accepted demo values:
    ///
    /// <list type="bullet">
    ///   <item>
    ///     <description>male</description>
    ///   </item>
    ///   <item>
    ///     <description>female</description>
    ///   </item>
    ///   <item>
    ///     <description>non-binary</description>
    ///   </item>
    /// </list>
    /// </remarks>
    /// <example>female</example>
    public required string Gender { get; set; } = string.Empty;
    }
