using Dapr.Actors.Runtime;

namespace UniqueNumberGenerator.Actors;

public class UniqueNumberActor : Actor, IUniqueNumberActor
{
    public const string StateName = "current-number";

    public UniqueNumberActor(ActorHost host) : base(host)
    {
    }

    public async Task<long> GetNextNumber()
    {
        // Actor turns are single threaded, so read-increment-write is safe without locking.
        var current = await StateManager.GetOrAddStateAsync(StateName, 0L);
        var next = current + 1;
        await StateManager.SetStateAsync(StateName, next);

        return next;
    }
}
