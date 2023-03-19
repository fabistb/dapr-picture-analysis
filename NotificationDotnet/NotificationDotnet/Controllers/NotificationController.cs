using Dapr;
using Microsoft.AspNetCore.Mvc;
using NotificationDotnet.Models;
using NotificationDotnet.Services;

namespace NotificationDotnet.Controllers;

[ApiController]
[Route("api/v1.0/[controller]")]
public class NotificationController : ControllerBase
{
    private readonly INotificationService _notificationService;

    public NotificationController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    [HttpPost]
    [Topic("messagebus", "notification-received")]
    public async Task<IActionResult> Notification([FromBody] NotificationMessage message)
    {
        await _notificationService.Process(message);

        return Ok();
    }
}