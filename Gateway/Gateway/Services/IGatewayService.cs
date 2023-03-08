using Gateway.Models;

namespace Gateway.Services;

public interface IGatewayService
{
    Task ProcessRequest(GatewayRequest request);
}