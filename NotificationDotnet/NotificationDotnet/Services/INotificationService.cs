using NotificationDotnet.Models;

namespace NotificationDotnet.Services;

public interface INotificationService
{
    Task Process(NotificationMessage message);
}