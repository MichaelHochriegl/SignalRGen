using SignalRGen.Generator;

namespace SignalRGen.Example.Contracts;

[HubClient(HubUri = "example")]
public interface IExampleHubClient
{
    Task ReceiveExampleCountUpdate(int count);
    Task ReceiveExampleCount2Update(int count, string foo);

    [ClientToServerMethod]
    Task<string> SendExampleMessage(string myClientMessage);

    [ClientToServerMethod]
    Task SendWithoutReturnType(string myClientMessage);
}

public record MyCustomType(string Hey, int Dude);