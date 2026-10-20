using AwesomeAssertions;
using Dapr.Actors;
using Dapr.Actors.Runtime;
using Moq;
using UniqueNumberGenerator.Actors;

namespace UniqueNumberGenerator.UnitTests.Actors;

[TestClass]
public class UniqueNumberActorTests
{
    private MockRepository _mockRepository = null!;
    private Mock<IActorStateManager> _stateManager = null!;
    private UniqueNumberActor _sut = null!;

    [TestInitialize]
    public void Setup()
    {
        _mockRepository = new MockRepository(MockBehavior.Strict);
        _stateManager = _mockRepository.Create<IActorStateManager>();

        var host = ActorHost.CreateForTest<UniqueNumberActor>(new ActorTestOptions { ActorId = new ActorId("test") });
        _sut = new UniqueNumberActor(host);
        SetStateManager(_sut, _stateManager.Object);
    }

    [TestCleanup]
    public void Cleanup() => _mockRepository.VerifyAll();

    [TestMethod]
    public async Task GetNextNumber_NoStateYet_ReturnsOneAndStoresIt()
    {
        _stateManager.Setup(s => s.GetOrAddStateAsync(UniqueNumberActor.StateName, 0L, It.IsAny<CancellationToken>()))
            .ReturnsAsync(0L);
        _stateManager.Setup(s => s.SetStateAsync(UniqueNumberActor.StateName, 1L, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _sut.GetNextNumber();

        result.Should().Be(1);
        _stateManager.Verify(s => s.SetStateAsync(UniqueNumberActor.StateName, 1L, It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    public async Task GetNextNumber_ExistingState_ReturnsIncrementedNumberAndStoresIt()
    {
        _stateManager.Setup(s => s.GetOrAddStateAsync(UniqueNumberActor.StateName, 0L, It.IsAny<CancellationToken>()))
            .ReturnsAsync(41L);
        _stateManager.Setup(s => s.SetStateAsync(UniqueNumberActor.StateName, 42L, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _sut.GetNextNumber();

        result.Should().Be(42);
        _stateManager.Verify(s => s.SetStateAsync(UniqueNumberActor.StateName, 42L, It.IsAny<CancellationToken>()), Times.Once);
    }

    // Actor.StateManager has an internal setter that the Dapr runtime uses; tests have to set it via reflection.
    private static void SetStateManager(Actor actor, IActorStateManager stateManager) =>
        typeof(Actor).GetProperty(nameof(Actor.StateManager))!.SetValue(actor, stateManager);
}
