using Microsoft.AspNetCore.SignalR;
using SignalRGen.Example.Contracts;

namespace SignalRGen.Example.Server.Api.Hubs;

public class ExampleHub : Hub<IExampleHubServerToClient>, IExampleHubClientToServer
{
    public async Task SendCount(int count) =>
        await Clients.All.ReceiveExampleCountUpdate(count);

    public Task<string> SendExampleMessage(string clientMessage)
    {
        return Task.FromResult($"Server responded to the client message: '{clientMessage}'");
    }

    public Task SendWithoutReturnType(string clientMessage)
    {
        Console.WriteLine($"Received: {clientMessage}");
        return Task.CompletedTask;
    }
}