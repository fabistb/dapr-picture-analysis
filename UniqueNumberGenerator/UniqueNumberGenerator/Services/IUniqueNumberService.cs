namespace UniqueNumberGenerator.Services;

public interface IUniqueNumberService
{
    Task<long> GetNextNumber();
}
