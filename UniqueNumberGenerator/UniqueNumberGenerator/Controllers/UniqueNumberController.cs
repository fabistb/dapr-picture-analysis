using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using UniqueNumberGenerator.Models;
using UniqueNumberGenerator.Services;

namespace UniqueNumberGenerator.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class UniqueNumberController : ControllerBase
{
    private readonly IUniqueNumberService _uniqueNumberService;

    public UniqueNumberController(IUniqueNumberService uniqueNumberService)
    {
        _uniqueNumberService = uniqueNumberService;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var number = await _uniqueNumberService.GetNextNumber();

        return Ok(new UniqueNumberResponse(number));
    }
}
