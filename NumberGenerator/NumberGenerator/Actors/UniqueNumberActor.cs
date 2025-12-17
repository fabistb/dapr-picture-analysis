using Dapr.Actors;
using Dapr.Actors.Runtime;

namespace NumberGenerator.Actors;

public class UniqueNumberActor : Actor, IUniqueNumberActor
{
    private const string StateKeyName = "currentNumber";

    public UniqueNumberActor(ActorHost host) : base(host)
    {
    }

    public async Task<long> GetNextNumberAsync()
    {
        var currentNumber = await StateManager.GetOrAddStateAsync(StateKeyName, 0L);
        var nextNumber = currentNumber + 1;
        await StateManager.SetStateAsync(StateKeyName, nextNumber);
        await StateManager.SaveStateAsync();
        return nextNumber;
    }
}
