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
    private static readonly ActorId ExpectedActorId = new("unique-number-generator");

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
            .Setup(f => f.CreateActorProxy<IUniqueNumberActor>(ExpectedActorId, nameof(UniqueNumberActor), null))
            .Returns(_actor.Object);
        _actor.Setup(a => a.GetNextNumber()).ReturnsAsync(number);

        var result = await _sut.GetNextNumber();

        result.Should().Be(number);
        _actor.Verify(a => a.GetNextNumber(), Times.Once);
    }

    [TestMethod]
    public async Task GetNextNumber_CalledTwice_AlwaysUsesUniqueNumberGeneratorActorId()
    {
        _actorProxyFactory
            .Setup(f => f.CreateActorProxy<IUniqueNumberActor>(ExpectedActorId, nameof(UniqueNumberActor), null))
            .Returns(_actor.Object);
        _actor.SetupSequence(a => a.GetNextNumber()).ReturnsAsync(1L).ReturnsAsync(2L);

        var first = await _sut.GetNextNumber();
        var second = await _sut.GetNextNumber();

        first.Should().Be(1);
        second.Should().Be(2);
        _actorProxyFactory.Verify(
            f => f.CreateActorProxy<IUniqueNumberActor>(ExpectedActorId, nameof(UniqueNumberActor), null),
            Times.Exactly(2));
    }
}
