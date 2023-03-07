using FileService.Models;

namespace FileService.Services;

public interface IFileService
{
    Task<FileResponse> Save(FileRequest request);

    Task<FileRequest> Get(string fileName);
}