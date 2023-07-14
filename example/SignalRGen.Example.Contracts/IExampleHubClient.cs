using SignalRGen.Generator;

namespace SignalRGen.Example.Contracts;

[HubClient(HubUri = "examples")]
public interface IExampleHubClient
{
    Task ReceiveExampleCountUpdate(int count);
}