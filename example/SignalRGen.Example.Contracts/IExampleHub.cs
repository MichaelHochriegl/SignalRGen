using SignalRGen.Abstractions;
using SignalRGen.Abstractions.Attributes;

namespace SignalRGen.Example.Contracts;

[HubClient(HubUri = "example")]
public interface IExampleHub : IBidirectionalHub<IExampleHubServerToClient, IExampleHubClientToServer>
{
}

[HubClient(HubUri = "example-server-to-client-only")]
public interface IExampleServerToClientOnlyHub : IServerToClientHub<IExampleHubServerToClient>
{
}

public record MyCustomType(string Hey, int Dude);