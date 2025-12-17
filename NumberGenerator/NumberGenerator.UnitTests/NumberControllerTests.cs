using AutoFixture;
using Dapr.Actors;
using Dapr.Actors.Client;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NumberGenerator.Actors;
using NumberGenerator.Controllers;

namespace NumberGenerator.UnitTests;

[TestClass]
public class NumberControllerTests
{
    private Mock<IActorProxyFactory> _actorProxyFactoryMock = null!;
    private Mock<IUniqueNumberActor> _actorMock = null!;
    private NumberController _controller = null!;
    private Fixture _fixture = null!;

    [TestInitialize]
    public void Setup()
    {
        _fixture = new Fixture();
        _actorProxyFactoryMock = new Mock<IActorProxyFactory>();
        _actorMock = new Mock<IUniqueNumberActor>();
        _controller = new NumberController(_actorProxyFactoryMock.Object);
    }

    [TestMethod]
    public async Task GetNextNumber_ReturnsOkResultWithNumber()
    {
        // Arrange
        var expectedNumber = _fixture.Create<long>();
        
        _actorProxyFactoryMock
            .Setup(x => x.CreateActorProxy<IUniqueNumberActor>(It.IsAny<ActorId>(), It.IsAny<string>(), It.IsAny<ActorProxyOptions>()))
            .Returns(_actorMock.Object);

        _actorMock
            .Setup(x => x.GetNextNumberAsync())
            .ReturnsAsync(expectedNumber);

        // Act
        var result = await _controller.GetNextNumber();

        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
        
        var okResult = result.Result as OkObjectResult;
        Assert.IsNotNull(okResult);
        Assert.AreEqual(expectedNumber, okResult.Value);
    }

    [TestMethod]
    public async Task GetNextNumber_CallsActorProxyWithCorrectParameters()
    {
        // Arrange
        var expectedNumber = _fixture.Create<long>();
        
        _actorProxyFactoryMock
            .Setup(x => x.CreateActorProxy<IUniqueNumberActor>(It.IsAny<ActorId>(), It.IsAny<string>(), It.IsAny<ActorProxyOptions>()))
            .Returns(_actorMock.Object);

        _actorMock
            .Setup(x => x.GetNextNumberAsync())
            .ReturnsAsync(expectedNumber);

        // Act
        await _controller.GetNextNumber();

        // Assert
        _actorProxyFactoryMock.Verify(
            x => x.CreateActorProxy<IUniqueNumberActor>(
                It.Is<ActorId>(id => id.GetId() == "unique-number-generator"),
                nameof(UniqueNumberActor),
                It.IsAny<ActorProxyOptions>()),
            Times.Once);

        _actorMock.Verify(x => x.GetNextNumberAsync(), Times.Once);
    }
}
