using Gateway.Models;

namespace Gateway.Daos;

public interface IFileDao
{
    Task<FileResponse> Save(string fileType, string base64);
}