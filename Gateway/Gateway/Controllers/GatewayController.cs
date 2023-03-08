using Gateway.Models;
using Gateway.Services;
using Microsoft.AspNetCore.Mvc;

namespace Gateway.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class GatewayController : ControllerBase
{
    private readonly IGatewayService _gatewayService;

    public GatewayController(IGatewayService gatewayService)
    {
        _gatewayService = gatewayService;
    }
    
    [HttpPost]
    public async Task<IActionResult> ReceiveRequest([FromBody] GatewayRequest request)
    {
        

        return Accepted();
    }
}