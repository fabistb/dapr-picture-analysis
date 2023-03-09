using System.Text;
using System.Text.Json;
using CommunityToolkit.HighPerformance;
using Dapr.Client;
using FileService.Models;

namespace FileService.Services;

public class FileService : IFileService
{
    private readonly DaprClient _daprClient;

    public FileService(DaprClient daprClient)
    {
        _daprClient = daprClient;
    }

    public async Task<FileResponse> Save(FileRequest request)
    {
        var fileName = $"{Guid.NewGuid()}.{request.FileType}";

        var bindingRequest = new BindingRequest("file-entry-storage-binding", "create")
        {
            Data = JsonSerializer.SerializeToUtf8Bytes(request.Base64),
            Metadata =
            {
                {"blobName", fileName}
            }
        };

        _ = await _daprClient.InvokeBindingAsync(bindingRequest);
        var fileResponse = new FileResponse(fileName);
        
        return fileResponse;
    }

    public async Task<FileRequest> Get(string fileName)
    {
        var bindingRequest = new BindingRequest("file-entry-storage-binding", "get")
        {
            Data = null,
            Metadata =
            {
                {"blobName", fileName}
            }
        };

        var blobResponse = await _daprClient.InvokeBindingAsync(bindingRequest);
        var fileRequest = new FileRequest(Convert.ToBase64String(blobResponse.Data.ToArray()), null);

        return fileRequest;
    }
}