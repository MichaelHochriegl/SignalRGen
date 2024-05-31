using SignalRGen.Generator;

namespace SignalRGen.Example.Contracts;

[HubClient(HubUri = "example")]
public interface IExampleHubClient
{
    Task ReceiveExampleCountUpdate(int count);

    [ClientToServerMethod]
    Task<string> SendExampleMessage(string myClientMessage);

    [ClientToServerMethod]
    Task SendWithoutReturnType(string myClientMessage);
}

public record MyCustomType(string Hey, int Dude);