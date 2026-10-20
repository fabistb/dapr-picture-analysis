using Dapr.Actors;
using Dapr.Actors.Client;
using UniqueNumberGenerator.Actors;

namespace UniqueNumberGenerator.Services;

public class UniqueNumberService : IUniqueNumberService
{
    // A single actor instance guarantees that every number is handed out exactly once.
    private static readonly ActorId GeneratorActorId = new("unique-number-generator");

    private readonly IActorProxyFactory _actorProxyFactory;

    public UniqueNumberService(IActorProxyFactory actorProxyFactory)
    {
        _actorProxyFactory = actorProxyFactory;
    }

    public async Task<long> GetNextNumber()
    {
        var actor = _actorProxyFactory.CreateActorProxy<IUniqueNumberActor>(GeneratorActorId, nameof(UniqueNumberActor));

        return await actor.GetNextNumber();
    }
}
