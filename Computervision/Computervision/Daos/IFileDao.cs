namespace Computervision.Daos;

public interface IFileDao
{
    Task<string> GetPicture(string fileReference);
}