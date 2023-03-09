using System.Text.Json;
using Dapr.Client;
using Gateway.Models;
using Gateway.Models.Exceptions;

namespace Gateway.Daos;

public class FileDao : IFileDao
{
    private readonly DaprClient _daprClient;

    public FileDao(DaprClient daprClient)
    {
        _daprClient = daprClient;
    }

    public async Task<FileResponse> Save(string fileType, string base64)
    {
        var fileRequest = new FileRequest(base64, fileType);

        var serviceInvocationRequest = _daprClient.CreateInvokeMethodRequest("file-service", "api/v1.0/File", fileRequest);
        var response = await _daprClient.InvokeMethodWithResponseAsync(serviceInvocationRequest);

        if (!response.IsSuccessStatusCode)
        {
            throw new ServiceInvocationException($"Service invocation failed with status code: {response.StatusCode}");
        }

        var fileResponse = JsonSerializer.Deserialize<FileResponse>(await response.Content.ReadAsStringAsync(), 
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

        return fileResponse;
    }
}