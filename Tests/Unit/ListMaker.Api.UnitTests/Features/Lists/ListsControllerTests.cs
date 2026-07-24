using ListMaker.Api.Controllers;
using ListMaker.Api.Features.Lists;
using ListMaker.Contracts.Lists;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace ListMaker.Api.UnitTests.Features.Lists;

/// <summary>
/// Contains unit tests for <see cref="ListsController" />.
/// </summary>
[TestFixture]
public sealed class ListsControllerTests
    {
    /// <summary>
    /// Verifies that the generated people endpoint returns HTTP 200 with provider data.
    /// </summary>
    [Test]
    public void GetGeneratedPeople_WhenProviderReturnsPeople_ShouldReturnOkWithPeople ()
        {
        // Arrange
        IReadOnlyList<PersonListItemDto> expectedPeople =
        [
            new PersonListItemDto
                {
                Id = 1,
                Name = "Leila",
                Family = "Akbari",
                Age = 38,
                Gender = "Male"
                },
            new PersonListItemDto
                {
                Id = 2,
                Name = "Mina",
                Family = "Ghasemi",
                Age = 52,
                Gender = "Female"
                }
        ];

        var personListProviderMock = new Mock<IPersonListProvider>(MockBehavior.Strict);

        personListProviderMock
            .Setup(provider => provider.GetGeneratedPeople())
            .Returns(expectedPeople);

        var controller = new ListsController(personListProviderMock.Object);

        // Act
        ActionResult<IReadOnlyList<PersonListItemDto>> actionResult = controller.GetGeneratedPeople();

        // Assert
        OkObjectResult okResult = actionResult.Result
            .Should()
            .BeOfType<OkObjectResult>()
            .Subject;

        IReadOnlyList<PersonListItemDto> actualPeople = okResult.Value
            .Should()
            .BeAssignableTo<IReadOnlyList<PersonListItemDto>>()
            .Subject;

        actualPeople.Should().BeSameAs(expectedPeople);

        personListProviderMock.Verify(
            provider => provider.GetGeneratedPeople(),
            Times.Once);

        personListProviderMock.VerifyNoOtherCalls();
        }

    /// <summary>
    /// Verifies that the generated people endpoint returns HTTP 200 even when the provider returns an empty list.
    /// </summary>
    [Test]
    public void GetGeneratedPeople_WhenProviderReturnsEmptyList_ShouldReturnOkWithEmptyList ()
        {
        // Arrange
        IReadOnlyList<PersonListItemDto> expectedPeople = Array.Empty<PersonListItemDto>();

        var personListProviderMock = new Mock<IPersonListProvider>(MockBehavior.Strict);

        personListProviderMock
            .Setup(provider => provider.GetGeneratedPeople())
            .Returns(expectedPeople);

        var controller = new ListsController(personListProviderMock.Object);

        // Act
        ActionResult<IReadOnlyList<PersonListItemDto>> actionResult = controller.GetGeneratedPeople();

        // Assert
        OkObjectResult okResult = actionResult.Result
            .Should()
            .BeOfType<OkObjectResult>()
            .Subject;

        IReadOnlyList<PersonListItemDto> actualPeople = okResult.Value
            .Should()
            .BeAssignableTo<IReadOnlyList<PersonListItemDto>>()
            .Subject;

        actualPeople.Should().BeEmpty();
        actualPeople.Should().BeSameAs(expectedPeople);

        personListProviderMock.Verify(
            provider => provider.GetGeneratedPeople(),
            Times.Once);

        personListProviderMock.VerifyNoOtherCalls();
        }

    /// <summary>
    /// Verifies that the generated people endpoint returns exactly the provider output without remapping.
    /// </summary>
    [Test]
    public void GetGeneratedPeople_ShouldReturnExactlyProviderOutput ()
        {
        // Arrange
        IReadOnlyList<PersonListItemDto> expectedPeople =
        [
            new PersonListItemDto
                {
                Id = 10,
                Name = "Navid",
                Family = "Ebrahimi",
                Age = 44,
                Gender = "Female"
                }
        ];

        var personListProviderMock = new Mock<IPersonListProvider>(MockBehavior.Strict);

        personListProviderMock
            .Setup(provider => provider.GetGeneratedPeople())
            .Returns(expectedPeople);

        var controller = new ListsController(personListProviderMock.Object);

        // Act
        ActionResult<IReadOnlyList<PersonListItemDto>> actionResult = controller.GetGeneratedPeople();

        // Assert
        OkObjectResult okResult = actionResult.Result
            .Should()
            .BeOfType<OkObjectResult>()
            .Subject;

        okResult.Value
            .Should()
            .BeEquivalentTo(
                expectedPeople,
                options => options.WithStrictOrdering());

        personListProviderMock.Verify(
            provider => provider.GetGeneratedPeople(),
            Times.Once);

        personListProviderMock.VerifyNoOtherCalls();
        }
    }
