using Computervision.Daos;
using Computervision.Models;
using Dapr.Client;

namespace Computervision.Services;

public class ComputervisionService : IComputervisionService
{
    private readonly IFileDao _fileDao;
    private readonly IAnalysisService _analysisService;
    private readonly DaprClient _daprClient;

    public ComputervisionService(IFileDao fileDao, IAnalysisService analysisService, DaprClient daprClient)
    {
        _fileDao = fileDao;
        _analysisService = analysisService;
        _daprClient = daprClient;
    }
    
    public async Task ProcessImage(string fileReference)
    {
        var base64 = await _fileDao.GetPicture(fileReference);
        var imageTags = await _analysisService.AnalyzeImage(base64);

        var containsCat = imageTags.FirstOrDefault(x => x.Name.Contains("cat", StringComparison.CurrentCultureIgnoreCase));

        if (containsCat is {Score: > 0.5})
        {
            var notificationMessage = new NotificationMessage("The submitted picture probably contains a cat");
            
            await _daprClient.PublishEventAsync("messagebus", "notification-received", notificationMessage);
        }
    }
}