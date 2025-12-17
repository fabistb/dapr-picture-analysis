using Dapr.Actors;
using Dapr.Actors.Client;
using Microsoft.AspNetCore.Mvc;
using NumberGenerator.Actors;

namespace NumberGenerator.Controllers;

[ApiController]
[Route("api/v1.0/[controller]")]
public class NumberController : ControllerBase
{
    private const string ActorId = "unique-number-generator";
    private readonly IActorProxyFactory _actorProxyFactory;

    public NumberController(IActorProxyFactory actorProxyFactory)
    {
        _actorProxyFactory = actorProxyFactory;
    }

    [HttpGet]
    public async Task<ActionResult<long>> GetNextNumber()
    {
        var actorId = new ActorId(ActorId);
        var proxy = _actorProxyFactory.CreateActorProxy<IUniqueNumberActor>(actorId, nameof(UniqueNumberActor));
        var nextNumber = await proxy.GetNextNumberAsync();
        return Ok(nextNumber);
    }
}
