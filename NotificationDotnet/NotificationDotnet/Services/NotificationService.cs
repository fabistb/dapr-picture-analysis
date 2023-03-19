using System.Text;
using System.Text.Json;
using Dapr.Client;
using NotificationDotnet.Models;

namespace NotificationDotnet.Services;

public class NotificationService : INotificationService
{
    private readonly DaprClient _daprClient;

    public NotificationService(DaprClient daprClient)
    {
        _daprClient = daprClient;
    }
    
    public async Task Process(NotificationMessage message)
    {
        var bindingRequest = new BindingRequest("notification-storage", "create")
        {
            Data = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message)),
        };

        await _daprClient.InvokeBindingAsync(bindingRequest);
    }
}