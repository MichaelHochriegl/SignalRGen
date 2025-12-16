using SignalRGen.Abstractions;
using SignalRGen.Abstractions.Attributes;

namespace SignalRGen.Example.Contracts;

// This attribute tells SignalRGen that this interface is a Hub that needs to be generated.
// The generated Hub will have a URI of "chat". Defining a URI is mandatory and you could, optionally define a name 
// for the Hub. If you don't define a name, the Hub will be named after the interface via convention,
// in this example "ChatHubContractClient".
[HubClient(HubUri = "chat")]
public interface IChatHubContract : IBidirectionalHub<IChatHubServerToClient, IChatHubClientToServer>
{
    // As this is only the Contract interface, there are no methods allowed in this interface.
    // You have to describe your Hub in the `TServer` and `TClient` interfaces instead.
}

// This interface describes the flow of data from the server to the client.
public interface IChatHubServerToClient
{
    Task UserJoined(string user);
    Task UserLeft(string user);
    Task MessageReceived(ChatMessage message);
}

// This interface describes the flow of data from the client to the server.
public interface IChatHubClientToServer
{
    Task SendMessage(string message);
}

public record ChatMessage(string User, string Message, DateTimeOffset Timestamp);