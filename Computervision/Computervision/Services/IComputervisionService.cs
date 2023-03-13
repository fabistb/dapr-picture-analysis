namespace Computervision.Services;

public interface IComputervisionService
{
    Task ProcessImage(string fileReference);
}