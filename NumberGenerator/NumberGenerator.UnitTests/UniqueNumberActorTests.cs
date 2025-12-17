using AutoFixture;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NumberGenerator.Actors;

namespace NumberGenerator.UnitTests;

[TestClass]
public class UniqueNumberActorTests
{
    private Fixture _fixture = null!;

    [TestInitialize]
    public void Setup()
    {
        _fixture = new Fixture();
    }

    [TestMethod]
    public void UniqueNumberActor_CanBeInstantiated()
    {
        // Arrange & Act
        var actorHost = Dapr.Actors.Runtime.ActorHost.CreateForTest<UniqueNumberActor>("test-actor-id");
        var actor = new UniqueNumberActor(actorHost);

        // Assert
        Assert.IsNotNull(actor);
    }

    [TestMethod]
    public void UniqueNumberActor_ImplementsIUniqueNumberActor()
    {
        // Arrange & Act
        var actorHost = Dapr.Actors.Runtime.ActorHost.CreateForTest<UniqueNumberActor>("test-actor-id");
        var actor = new UniqueNumberActor(actorHost);

        // Assert
        Assert.IsInstanceOfType(actor, typeof(IUniqueNumberActor));
    }
}
