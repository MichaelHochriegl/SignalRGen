using SignalRGen.Generator;

namespace SignalRGen.Example.Contracts;

[HubClient(HubUri = "example")]
public interface IExampleHubClient
{
    Task ReceiveExampleCountUpdate(int count);
    Task ReceiveExampleCount2Update(int count, string foo);
}

public record MyCustomType(string Hey, int Dude);