# Generated Hub Client

When you define a SignalR interface with the `[HubClient]` attribute, `SignalRGen` automatically generates a 
strongly-typed client class for you. This page explains what gets generated and how to use the resulting client.

## Interface to Client: The Transformation Process

Let's examine how `SignalRGen` transforms your interface definition into a usable client class.

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

### Generated Client Class

From this interface, `SignalRGen` generates a class named `ChatHubContractClient` (removing the "I" prefix from the interface name and suffix it with "Client"). This class provides:

1. **Event-based pattern** for server-to-client methods
2. **Strongly-typed methods** for client-to-server calls
3. **Connection management** functionality

## Hub Client Features

### Connection Management

Each generated hub client includes methods to manage the SignalR connection:

::: code-group
```csharp
// Start the connection
Task StartAsync(
      Dictionary<string, string>? queryStrings = null,
      Dictionary<string, string>? headers = null,
      CancellationToken cancellationToken = default);

// Stop the connection
Task StopAsync(CancellationToken cancellationToken = default);

// Disposes the connection
ValueTask DisposeAsync();
```
:::

### Server-to-Client Methods

For each method in your `TServer` interface (in the example above the `IChatHubServerToClient` interface)
`SignalRGen` generates an event that you can subscribe to:

::: code-group
```csharp
// For the interface method:
// Task UserJoined(string user);

// SignalRGen generates:
public Func<string, Task>? OnUserJoined = default;
```
:::

The naming convention is to add the "On" prefix to the original method name.

### Client-to-Server Methods

For each method in your `TClient` interface (in the example above the `IChatHubClientToServer` interface), `SignalRGen` generates a method that calls the server:

::: code-group
```csharp
// For the interface method:
// Task SendMessage(string message);

// SignalRGen generates:
public Task InvokeSendMessageAsync(string message, CancellationToken ct = default)
{
    ValidateHubConnection();
    return _hubConnection!.InvokeAsync("SendMessage", message, cancellationToken: ct);
}
```
:::

The naming convention is to add the "Invoke" prefix and "Async" suffix to the original method name.

## Using the Generated Client

### Dependency Injection

Register the hub client in your DI container:

::: code-group
```csharp
// In Program.cs or Startup.cs
services.AddSignalRHubs(options => 
{
    options.HubBaseUri = new Uri("https://your-api.example.com/hubs");
})
.WithChatHubContractClient();  // Method name follows the pattern With[ClientName]
```
:::

### Basic Usage

Here's how to use the generated client in your application:

::: code-group
```csharp
public class ChatService
{
    private readonly ChatHubContractClient _hubClient;
    private readonly ILogger<ChatService> _logger;

    public ExampleService(ChatHubContractClient hubClient, ILogger<ExampleService> logger)
    {
        _hubClient = hubClient;
        _logger = logger;
    }

    public async Task Initialize()
    {
        // Subscribe to server-to-client events
        _hubClient.OnUserJoined += user =>
        {
            _logger.LogInformation("User '{Username} joined", user);
            return Task.CompletedTask;
        };

        // Start the connection
        await _hubClient.StartAsync();
    }

    public async Task SendMessage(string message)
    {
        // Call the method to send data to the server
        await _hubClient.InvokeSendMessageAsync(_message);
    }

    public async Task Cleanup()
    {
        // Unsubscribe from events
        _hubClient.OnUserJoined = null;

        // Stop the connection
        await _hubClient.StopAsync();
    }
}
```
:::

### Connection Events

The generated client also exposes events for connection state changes:

::: code-group
```csharp
// Subscribe to connection events
_hubClient.Reconnecting += error => 
{
    logger.LogWarning("Reconnecting due to: {Error}", error?.Message);
    return Task.CompletedTask;
};

_hubClient.Reconnected += connectionId => 
{
    logger.LogInformation("Reconnected with ID: {ConnectionId}", connectionId);
    return Task.CompletedTask;
};

_hubClient.Closed += error => 
{
    logger.LogWarning("Connection closed due to: {Error}", error?.Message);
    return Task.CompletedTask;
};
```
:::

## Client Lifecycle

Understanding the lifecycle of the generated hub client is important for proper usage:

1. **Registration**: The hub client is registered with your DI container when you call `.WithChatHubContractClient()`
2. **Injection**: The hub client is injected into your service
3. **Event Setup**: Subscribe to server-to-client events before starting the connection
4. **Connection**: Call `StartAsync()` to establish the connection
5. **Usage**: Call client-to-server methods and handle server-to-client events
6. **Cleanup**: Unsubscribe from events and call `StopAsync()` when done or use the auto-dispose provided by the DI container

By default, the hub client is registered as a `Singleton`, but you can change this using the [configuration options](../configuration/config-per-hub.md#hubclientlifetime).

## Working with Complex Types

`SignalRGen` supports complex types in both directions:

::: code-group
```csharp
[HubClient(HubUri = "complex-example")]
public interface IComplexTypeHubContract : IBidirectionalHub<IComplexTypeHubServerToClient, IComplexTypeHubClientToServer>
{
}

public interface IComplexTypeHubServerToClient
{
    // Server-to-client with complex type
    Task ReceiveComplexData(MyCustomType data);
}

public interface IComplexTypeHubClientToServer
{
    // Client-to-server with complex type
    Task<MyCustomType> SendComplexData(MyCustomType data);
}

public record MyCustomType(string Hey, int Dude);
```
:::

The generated client handles serialization and deserialization of these types automatically.

## Best Practices

1. **Event Handling**:
    - Always return `Task.CompletedTask` from event handlers if they don't perform async operations
    - Consider using weak event patterns for long-lived connections to prevent memory leaks

2. **Connection Management**:
    - Start connections when your application initializes, not on-demand for each operation
    - Calling `StartAsync` after the connection was already started does not fail, a `Task.CompletedTask` will be returned
    - `StartAsync`, `StopAsync` and `DisposeAsync` are **NOT** threadsafe
    - Use the `Reconnecting`, `Reconnected`, and `Closed` events to handle connection issues

3. **Resource Cleanup**:
    - Always stop connections when they're no longer needed
    - Unsubscribe from events before stopping connections

4. **Testing**:
    - Create test doubles (mocks/fakes) for the generated client to test your application logic

## Technical Details

The generated client:

- Uses Microsoft's `HubConnection` under the hood
- Implements automatic reconnection with configurable retry policies (see [default retry settings](../configuration/config-per-hub.md#default-behavior))
- Provides thread-safe event invocation
- Handles connection lifecycle and state management
- Uses asynchronous patterns throughout for non-blocking operation

## Troubleshooting

Common issues and solutions:

1. **Connection fails to establish**:
    - Verify the HubBaseUri is correct
    - Check if the server is running and accessible
    - Ensure proper authentication is configured if required

2. **Events not firing**:
    - Verify you've subscribed to events before starting the connection
    - Check if the connection is in the Connected state
    - Ensure server methods are calling the correct client methods

3. **Method invocation fails**:
    - Verify the connection is in the Connected state
    - Check for parameter type mismatches
    - Ensure proper authentication/authorization

For more detailed diagnostic information, enable SignalR logging:

::: code-group
```csharp
.WithChatHubContractClient(options => 
{
    options.HubConnectionBuilderConfiguration = builder => 
    {
        builder.ConfigureLogging(logging => 
        {
            logging.AddConsole();
            logging.SetMinimumLevel(LogLevel.Debug);
        });
    };
})
```
:::
