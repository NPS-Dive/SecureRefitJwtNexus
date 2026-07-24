using ListMaker.Api.Features.Lists;
using ListMaker.Contracts.Lists;
    
namespace ListMaker.Api.UnitTests.Features.Lists;

/// <summary>
/// Contains unit tests for <see cref="StablePersonListProvider" />.
/// </summary>
[TestFixture]
public sealed class StablePersonListProviderTests
    {
    /// <summary>
    /// Verifies that the provider always returns exactly 50 generated people.
    /// </summary>
    [Test]
    public void GetGeneratedPeople_ShouldReturnExactlyFiftyPeople ()
        {
        // Arrange
        var provider = new StablePersonListProvider();

        // Act
        IReadOnlyList<PersonListItemDto> people = provider.GetGeneratedPeople();

        // Assert
        people.Should().HaveCount(50);
        }

    /// <summary>
    /// Verifies that generated IDs are stable, sequential, and start from 1.
    /// </summary>
    [Test]
    public void GetGeneratedPeople_ShouldReturnSequentialIdsFromOneToFifty ()
        {
        // Arrange
        var provider = new StablePersonListProvider();

        // Act
        IReadOnlyList<PersonListItemDto> people = provider.GetGeneratedPeople();

        // Assert
        people.Select(person => person.Id)
            .Should()
            .Equal(Enumerable.Range(1, 50));
        }

    /// <summary>
    /// Verifies that all generated people contain valid basic data.
    /// </summary>
    [Test]
    public void GetGeneratedPeople_ShouldReturnPeopleWithValidData ()
        {
        // Arrange
        var provider = new StablePersonListProvider();

        string[] acceptedGenders =
        [
            "Male",
            "Female",
            "non-Binary"
        ];

        // Act
        IReadOnlyList<PersonListItemDto> people = provider.GetGeneratedPeople();

        // Assert
        people.Should().OnlyContain(person => person.Id >= 1);
        people.Should().OnlyContain(person => !string.IsNullOrWhiteSpace(person.Name));
        people.Should().OnlyContain(person => !string.IsNullOrWhiteSpace(person.Family));
        people.Should().OnlyContain(person => person.Age >= 18 && person.Age <= 65);
        people.Should().OnlyContain(person => acceptedGenders.Contains(person.Gender));
        }

    /// <summary>
    /// Verifies that repeated calls on the same provider return the same cached list instance.
    /// </summary>
    [Test]
    public void GetGeneratedPeople_WhenCalledMultipleTimesOnSameProvider_ShouldReturnSameCachedInstance ()
        {
        // Arrange
        var provider = new StablePersonListProvider();

        // Act
        IReadOnlyList<PersonListItemDto> firstResult = provider.GetGeneratedPeople();
        IReadOnlyList<PersonListItemDto> secondResult = provider.GetGeneratedPeople();

        // Assert
        secondResult.Should().BeSameAs(firstResult);
        }

    /// <summary>
    /// Verifies that different provider instances generate equivalent deterministic data.
    /// </summary>
    [Test]
    public void GetGeneratedPeople_WhenProviderIsRecreated_ShouldReturnEquivalentStableData ()
        {
        // Arrange
        var firstProvider = new StablePersonListProvider();
        var secondProvider = new StablePersonListProvider();

        // Act
        IReadOnlyList<PersonListItemDto> firstResult = firstProvider.GetGeneratedPeople();
        IReadOnlyList<PersonListItemDto> secondResult = secondProvider.GetGeneratedPeople();

        // Assert
        secondResult.Should().BeEquivalentTo(
            firstResult,
            options => options.WithStrictOrdering());
        }

    /// <summary>
    /// Verifies a few fixed records to protect the deterministic seed behavior from accidental changes.
    /// </summary>
    [Test]
    public void GetGeneratedPeople_ShouldKeepExpectedSeededSnapshotValues ()
    {
        // Arrange
        var provider = new StablePersonListProvider();

        // Act
        IReadOnlyList<PersonListItemDto> people = provider.GetGeneratedPeople();

        // Assert
        people[0].Should().BeEquivalentTo(
            new PersonListItemDto
            {
                Id = 1,
                Name = "Leila",
                Family = "Akbari",
                Age = 38,
                Gender = "Male"
            });

        people[24].Should().BeEquivalentTo(
            new PersonListItemDto
            {
                Id = 25,
                Name = "Navid",
                Family = "Ebrahimi",
                Age = 44,
                Gender = "Female"
            });

        people[49].Should().BeEquivalentTo(
            new PersonListItemDto
            {
                Id = 50,
                Name = "Mina",
                Family = "Ghasemi",
                Age = 52,
                Gender = "Female"
            });
    }
    }
