using Asp.Versioning;
using FileService.Models;
using FileService.Services;
using Microsoft.AspNetCore.Mvc;

namespace FileService.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class FileController : ControllerBase
{
    private readonly IFileService _fileService;

    public FileController(IFileService fileService)
    {
        _fileService = fileService;
    }

    [HttpPost]
    public async Task<IActionResult> Save([FromBody] FileRequest request)
    {
        var fileResponse = await _fileService.Save(request);

        return Ok(fileResponse);
    }
    
    [HttpGet("{fileName}")]
    public async Task<IActionResult> Get(string fileName)
    {
        var fileRequest = await _fileService.Get(fileName);

        return Ok(fileRequest);
    }
}