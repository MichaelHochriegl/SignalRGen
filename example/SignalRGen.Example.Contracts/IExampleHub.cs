using SignalRGen.Abstractions;
using SignalRGen.Abstractions.Attributes;

namespace SignalRGen.Example.Contracts;

[HubClient(HubUri = "example")]
public interface IExampleHub : IBidirectionalHub<IExampleHubServerToClient, IExampleHubClientToServer>
{
}

public interface IExampleHubClientToServer
{
    Task<string> SendExampleMessage(string myClientMessage);
    
    Task SendWithoutReturnType(string myClientMessage);
}

public interface IExampleHubServerToClient
{
    Task ReceiveExampleCountUpdate(int count);
}

public record MyCustomType(string Hey, int Dude);