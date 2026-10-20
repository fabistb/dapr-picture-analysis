using AutoFixture;
using AwesomeAssertions;
using Dapr.Actors;
using Dapr.Actors.Client;
using Moq;
using UniqueNumberGenerator.Actors;
using UniqueNumberGenerator.Services;

namespace UniqueNumberGenerator.UnitTests.Services;

[TestClass]
public class UniqueNumberServiceTests
{
    private readonly Fixture _fixture = new();
    private MockRepository _mockRepository = null!;
    private Mock<IActorProxyFactory> _actorProxyFactory = null!;
    private Mock<IUniqueNumberActor> _actor = null!;
    private UniqueNumberService _sut = null!;

    [TestInitialize]
    public void Setup()
    {
        _mockRepository = new MockRepository(MockBehavior.Strict);
        _actorProxyFactory = _mockRepository.Create<IActorProxyFactory>();
        _actor = _mockRepository.Create<IUniqueNumberActor>();
        _sut = new UniqueNumberService(_actorProxyFactory.Object);
    }

    [TestCleanup]
    public void Cleanup() => _mockRepository.VerifyAll();

    [TestMethod]
    public async Task GetNextNumber_ActorReturnsNumber_ReturnsActorNumber()
    {
        var number = _fixture.Create<long>();
        _actorProxyFactory
            .Setup(f => f.CreateActorProxy<IUniqueNumberActor>(It.IsAny<ActorId>(), nameof(UniqueNumberActor), null))
            .Returns(_actor.Object);
        _actor.Setup(a => a.GetNextNumber()).ReturnsAsync(number);

        var result = await _sut.GetNextNumber();

        result.Should().Be(number);
        _actor.Verify(a => a.GetNextNumber(), Times.Once);
    }

    [TestMethod]
    public async Task GetNextNumber_CalledTwice_AlwaysUsesSameActorId()
    {
        var actorIds = new List<ActorId>();
        _actorProxyFactory
            .Setup(f => f.CreateActorProxy<IUniqueNumberActor>(It.IsAny<ActorId>(), nameof(UniqueNumberActor), null))
            .Callback<ActorId, string, ActorProxyOptions?>((id, _, _) => actorIds.Add(id))
            .Returns(_actor.Object);
        _actor.Setup(a => a.GetNextNumber()).ReturnsAsync(1L);

        await _sut.GetNextNumber();
        await _sut.GetNextNumber();

        actorIds.Should().HaveCount(2).And.AllBeEquivalentTo(actorIds[0]);
    }
}
