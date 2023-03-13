using Dapr.Client;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;

namespace Computervision.Services;

public class AnalysisService : IAnalysisService
{
    private readonly IConfiguration _configuration;
    private readonly DaprClient _daprClient;

    public AnalysisService(IConfiguration configuration, DaprClient daprClient)
    {
        _configuration = configuration;
        _daprClient = daprClient;
    }
    
    public async Task<List<Category>> AnalyzeImage(string base64)
    {
        var cognitiveSecretKey = await _daprClient.GetSecretAsync("secretstore", "cognitive-service-key");
        
        var client = new ComputerVisionClient(new ApiKeyServiceClientCredentials(cognitiveSecretKey.First().Value))
        {
            Endpoint = _configuration["COGNITIVE_SERVICE_URL"]
        };

        var visualFeatureTypes = new List<VisualFeatureTypes?>
        {
            VisualFeatureTypes.Categories
        };
        
        var stream = new MemoryStream(Convert.FromBase64String(base64));
        var analysisResult  = await client.AnalyzeImageInStreamAsync(stream, visualFeatureTypes);
        
        return  analysisResult.Categories.ToList();
    }
}