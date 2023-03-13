using Computervision.Models;
using Computervision.Services;
using Dapr;
using Microsoft.AspNetCore.Mvc;

namespace Computervision.Controllers;

[ApiController]
[Route("api/v1.0/[controller]")]
public class ComputervisionController
{
    private readonly IComputervisionService _computervisionService;

    public ComputervisionController(IComputervisionService computervisionService)
    {
        _computervisionService = computervisionService;
    }
    
    [Topic("messagebus", "message-received")]
    [HttpPost]
    public async Task<IActionResult> ProcessImage([FromBody] Message request)
    {
        await _computervisionService.ProcessImage(request.FileReference);

        return new OkResult();
    }
}   