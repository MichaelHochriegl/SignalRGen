using Microsoft.AspNetCore.SignalR;
using SignalRGen.Example.Contracts;
using SignalRGen.Generator;

namespace SignalRGen.Example.Server.Api.Hubs;

public class ExampleHub : Hub<IExampleHubClient>
{
    public async Task SendCount(int count) =>
        await Clients.All.ReceiveExampleCountUpdate(count);
}