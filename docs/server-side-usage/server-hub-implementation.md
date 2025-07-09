# Server Hub Implementation

`SignalRGen` currently doesn't generate any Hubs automatically, but nevertheless it helps you with implementing
a type-safe SignalR Hub.

## Interface to Server

### Starting with an Interface
Here's a typical SignalR interface definition:

::: code-group
```csharp
using SignalRGen.Abstractions;
using SignalRGen.Abstractions.Attributes;

namespace SignalRGen.Example.Contracts;

[HubClient(HubUri = "chat")]
public interface IChatHubContract : IBidirectionalHub<IChatHubServerToClient, IChatHubClientToServer>
{
}

public interface IChatHubServerToClient
{
    Task UserJoined(string user);
    Task UserLeft(string user);
    Task MessageReceived(string user, string message);
}

public interface IChatHubClientToServer
{
    Task SendMessage(string message);
}
```
:::

### Writing the Hub

With this interface, we can implement our Hub with type-safety.
In our server project we will create a new class representing our Hub:

::: code-group
```csharp
using Microsoft.AspNetCore.SignalR;
using SignalRGen.Example.Contracts;

namespace SignalRGen.Example.Server.Api.Hubs;

// This is our Hub on the server side.
// We use the two interfaces we defined above to also make the server side typesafe.
public class ChatHub : Hub<IChatHubServerToClient>, IChatHubClientToServer
{
    // We use the `OnConnectedAsync` method to notify the other clients that a new user joined.
    public override Task OnConnectedAsync()
    {
        // The `UserJoined` is available as a method because we use the `IChatHubServerToClient` interface
        // in `Hub<IChatHubServerToClient>`
        Clients.Others.UserJoined(Context.UserIdentifier ?? "Anonymous");
        return base.OnConnectedAsync();
    }

    // This method must be implemented here, as we implement the interface `IChatHubClientToServer`
    public Task SendMessage(string message)
    {
        var user = Context.UserIdentifier ?? "Anonymous";
        return Clients.Others.MessageReceived(user, message);
    }

    // We use the `OnDisconnectedAsync` method to notify the other clients that a user left.
    public override Task OnDisconnectedAsync(Exception? exception)
    {
        // The `UserLeft` is available as a method because we use the `IChatHubServerToClient` interface
        // in `Hub<IChatHubServerToClient>`
        Clients.Others.UserLeft(Context.UserIdentifier ?? "Anonymous");
        return base.OnDisconnectedAsync(exception);
    }
}
```
:::

### Wiring up the Hub

We have to tell our server project to listen on the proper URI and route to our Hub.
For this we have to map it in the `Program.cs`:

::: code-group
```csharp
// To wire up our ChatHub, we have to map it here.
// We use the `HubUri` from the generated client.
// This ensures that both client and server will use the same route.
app.MapHub<ChatHub>($"{ChatHubContractClient.HubUri}");
```
:::

For this step we can use the [generated HubClient](../client-side-usage/generated-hub-clients) to have a refactor save URI.
