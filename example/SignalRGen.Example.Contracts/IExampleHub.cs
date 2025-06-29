using SignalRGen.Abstractions;
using SignalRGen.Abstractions.Attributes;

namespace SignalRGen.Example.Contracts;

[HubClient(HubUri = "example")]
public interface IExampleHub : IBidirectionalHub<IExampleHubServerToClient, IExampleHubClientToServer>
{
    Task FooBar();
    Task Foo1();
}

[HubClient(HubUri = "example-server-to-client-only")]
public interface IExampleServerToClientOnlyHub : IServerToClientHub<IExampleHubServerToClient>
{
    Task Bar();
}

public record MyCustomType(string Hey, int Dude);