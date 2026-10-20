using Dapr.Actors;

namespace UniqueNumberGenerator.Actors;

public interface IUniqueNumberActor : IActor
{
    Task<long> GetNextNumber();
}
