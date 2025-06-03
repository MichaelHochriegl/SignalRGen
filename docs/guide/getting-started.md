# Getting Started with SignalRGen

SignalRGen simplifies working with SignalR in .NET applications by generating strongly typed clients from simple interface definitions.
This guide will walk you through creating a basic SignalR communication setup backed by `SignalRGen`.

::: tip
If you want a more elaborate rundown, head over to the [detailed tutorial](./detailed-tutorial.md).
:::

## Overview

We'll build a simple ping-pong example with:
- A server that responds to ping messages
- A client that sends pings and receives pongs
- A shared interface defining our contract

## Quick Installation

Add `SignalRGen` to your shared interface project:

::: code-group
```shell
dotnet add package SignalRGen
```
:::

Then modify your package reference in the .csproj file:

```xml
<PackageReference Include="SignalRGen" Version="1.0.0">
    <PrivateAssets>all</PrivateAssets>
    <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
</PackageReference>
```

## Define Your SignalR Interface

Create an interface that describes your Hub's communication:

::: code-group
```csharp
using SignalRGen.Generator;

namespace MySharedInterface;

[HubClient(HubUri = "ping-pong")]
public interface IPingPongHub
{
    // Client-to-server method (client calls this)
    [ClientToServerMethod]
    Task Ping(string message);

    // Server-to-client method (server calls this)
    Task Pong(string answer);
}
```
:::

That's it! `SignalRGen` will automatically generate the client implementation.

## Server-Side Implementation

Create a Hub class that inherits from both `Hub<IPingPongHub>` and `IPingPongHub`:

::: code-group
```csharp
public class PongHub : Hub<IPingPongHub>, IPingPongHub
{
    private readonly ILogger<PongHub> _logger;

    public PongHub(ILogger<PongHub> logger)
    {
        _logger = logger;
    }
    
    public Task Ping(string message)
    {
        _logger.LogInformation("Received Ping: {Message}", message);
        return Clients.All.Pong("Hey, here is the server talking!");
    }

    public Task Pong(string answer)
    {
        throw new InvalidOperationException("This is a Server-to-Client method!");
    }
}
```
:::

Remember to register your Hub in `Program.cs`:

::: code-group
```csharp
app.MapHub<PingPongHub>($"/{PingPongHub.HubUri}");
```
:::

## Client-Side Usage

### Register the Hub

Configure the hub in your client's `Program.cs`:

::: code-group
```csharp
builder.Services.AddSignalRHubs(c => c.HubBaseUri = new Uri("http://localhost:5160"))
    .WithPingPongHub();
```
:::

### Use the Generated Client

The generated `PingPongHub` class can now be injected into your services:

::: code-group
```csharp
public class Worker : IHostedService
{
    private readonly ILogger<Worker> _logger;
    private readonly PingPongHub _hub;  // Use the generated class, not the interface

    public Worker(ILogger<Worker> logger, PingPongHub hub)
    {
        _logger = logger;
        _hub = hub;
    }
    
    public Task StartAsync(CancellationToken cancellationToken)
    {
        // Subscribe to server-to-client events
        _hub.OnPong += answer => {
            _logger.LogInformation("Received Pong: {Answer}", answer);
            return Task.CompletedTask;
        };

        return _hub.StartAsync(cancellationToken: cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return _hub.StopAsync(cancellationToken: cancellationToken);
    }
}
```
:::

### Call Server Methods

Invoke client-to-server methods like this:

::: code-group
```csharp
app.MapGet("/ping", async ([FromServices] PingPongHub hub) =>
{
    await hub.InvokePingAsync("Hello from the client!");
});
```
:::

## Key Points to Remember

- The interface (`IPingPongHub`) defines the contract between client and server
- Methods without `[ClientToServerMethod]` are server-to-client callbacks
- Methods with `[ClientToServerMethod]` are client-to-server calls
- Use the generated class (`PingPongHub`) in your client code, not the interface
- The server implements both the interface and inherits from `Hub<TInterface>`

## Next Steps

For more advanced configuration options, see the [Configuration Guide](../configuration/config-overview.md).

## Complete Example

For the full step-by-step example including project setup, see our [detailed tutorial](./detailed-tutorial.md).
```