using Dapr.Actors;

namespace NumberGenerator.Actors;

public interface IUniqueNumberActor : IActor
{
    Task<long> GetNextNumberAsync();
}
