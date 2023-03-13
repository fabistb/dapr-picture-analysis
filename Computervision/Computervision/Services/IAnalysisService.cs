using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;

namespace Computervision.Services;

public interface IAnalysisService
{
    Task<List<Category>> AnalyzeImage(string base64);
}