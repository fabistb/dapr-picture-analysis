using Dapr.Client;
using Gateway.Daos;
using Gateway.Models;

namespace Gateway.Services;

public class GatewayService : IGatewayService
{
    private readonly IFileDao _fileDao;
    private readonly DaprClient _daprClient;

    public GatewayService(IFileDao fileDao, DaprClient daprClient)
    {
        _fileDao = fileDao;
        _daprClient = daprClient;
    }

    public async Task ProcessRequest(GatewayRequest request)
    {
        var lastPointIndex = request.FileName.LastIndexOf('.');
        var fileType = request.FileName[(lastPointIndex + 1)..];
        
        var fileResponse = await _fileDao.Save(fileType, request.Base64);

        var pubSubMessage = new Message(fileResponse.FileName);

        await _daprClient.PublishEventAsync("messagebus", "message-received", pubSubMessage);
    }
}