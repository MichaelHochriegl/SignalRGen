# Hub Interface Definition

`SignalRGen` uses interfaces to define the contract between your client and server. This page explains how to create these interfaces and the attributes you'll use to control the generation process.

## Types of Hubs

`SignalRGen` supports three types of hub communication patterns:

### `IBidirectionalHub<TServer, TClient>`
Enables both server-to-client and client-to-server communication.

```csharp
[HubClient(HubUri = "chat")]
public interface IChatHubContract : IBidirectionalHub<IChatHubServerToClient, IChatHubClientToServer>
{
    // Contract interface - no methods allowed here
}

public interface IChatHubServerToClient
{
    Task MessageReceived(string user, string message);
    Task UserJoined(string user);
}

public interface IChatHubClientToServer
{
    Task SendMessage(string message);
    Task<List<string>> GetActiveUsers();
}
```

### `IServerToClientHub<TServer>`
One-way server-to-client communication.

```csharp
[HubClient(HubUri = "notifications")]
public interface INotificationHubContract : IServerToClientHub<INotificationHubServerToClient>
{
}

public interface INotificationHubServerToClient
{
    Task NotificationReceived(string title, string message);
    Task SystemAlert(string level, string details);
}
```

### `IClientToServerHub<TClient>`
One-way client-to-server communication.

```csharp
[HubClient(HubUri = "telemetry")]
public interface ITelemetryHubContract : IClientToServerHub<ITelemetryHubClientToServer>
{
}

public interface ITelemetryHubClientToServer
{
    Task LogEvent(string eventName, Dictionary<string, object> properties);
    Task ReportError(string error, string stackTrace);
}
```

## The HubClient Attribute

Every SignalR interface must be marked with the `[HubClient]` attribute:

```csharp
using SignalRGen.Generator;

namespace MyApp.Contracts;

[HubClient(HubUri = "example-hub")]
public interface IExampleHubContract : IBidirectionalHub<IExampleHubServerToClient, IExampleHubClientToServer>
{
    // This interface CANNOT have methods defined
}

public interface IExampleHubServerToClient
{
    // Server-to-client methods
}

public interface IExampleHubClientToServer
{
    // Client-to-server methods
}
```

:::tip 💡 Hint for naming
Throughout this documentation you will see the actual interface that `SignalRGen` uses to generate are named `XyzContract`.
The interfaces used as `TServer` and/or `TClient` are named as `AbcServerToClient`/`AbcClientToServer` for easier distinction.
:::

### HubClient Attribute Properties

| Property | Type | Required | Description | Example |
|----------|:----:|:--------:|-------------|---------|
| `HubUri` | `string` | ✅ | The relative path where the hub is hosted on the server | `"chat"`, `"api/notifications"` |
| `HubName` | `string` | ❌ | Custom name for the generated client class | `"CustomChatClient"` |

### URI Construction

The full hub URI is constructed by combining:
1. **Global `HubBaseUri`** from configuration: `https://api.example.com`
2. **`HubUri`** from the attribute: `chat`
3. **Result**: `https://api.example.com/chat`

:::tip 💡 URI Best Practices
- Use lowercase, hyphen-separated paths: `user-notifications`
- Avoid leading/trailing slashes in `HubUri`
- Keep paths short and descriptive
:::

### Naming Examples

```csharp
// Basic usage - generates "ExampleHubContractClient"
[HubClient(HubUri = "example")]
public interface IExampleHubContract { }

// Custom name - generates "CustomChatClient"
[HubClient(HubUri = "chat", HubName = "CustomChatClient")]
public interface IChatHubContract { }
```

## Defining Methods

### Server-to-Client Methods

Methods the server calls on connected clients. Defined in the `TServer` interface.

```csharp
public interface IExampleHubServerToClient
{
    // Simple notification
    Task UserJoined(string username);
    
    // Complex data
    Task MessageReceived(ChatMessage message);
    
    // Multiple parameters
    Task GameStateUpdated(int playerId, Vector3 position, float health);
}
```

**Requirements:**
- Must return `Task`
- Can have any number and type of parameters
- Exposed as events in the generated client

:::warning ⚠️ CancellationToken
Don't include `CancellationToken` parameters - `SignalRGen` adds these automatically.
:::

### Client-to-Server Methods

Methods the client calls on the server. Defined in the `TClient` interface.

```csharp
public interface IExampleHubClientToServer
{
    // Fire-and-forget
    Task SendMessage(string message);
    
    // Request-response
    Task<List<string>> GetActiveUsers();
    Task<UserProfile> GetUserProfile(string userId);
    
    // Complex operations
    Task<GameResult> SubmitMove(int gameId, GameMove move);
}
```

**Requirements:**
- Must return `Task` (no result) or `Task<T>` (with result)
- Can have any number and type of parameters
- Exposed as methods in the generated client

::: info IDE Support
`SignalRGen` includes analyzers and code fixes to help with method definitions:
- Validates return types
- Suggests fixes for common mistakes
- Provides quick actions for interface generation
:::

## Parameter and Return Types

### Supported Types

**Primitive Types:**
```csharp
Task ReceiveCount(int count);
Task ReceiveMessage(string message);
Task ReceiveFlag(bool isActive);
Task ReceiveAmount(decimal amount);
Task ReceiveTimestamp(DateTime timestamp);
```

**Nullable Types:**
```csharp
Task ReceiveOptionalValue(int? maybeValue);
Task ReceiveOptionalUser(User? user);
```

**Collections:**
```csharp
Task ReceiveList(List<string> items);
Task ReceiveArray(string[] items);
Task ReceiveDictionary(Dictionary<string, int> scores);
Task ReceiveSet(HashSet<string> uniqueItems);
```

**Complex Types:**
```csharp
// Custom classes
public class ChatMessage
{
    public string Content { get; set; }
    public string Sender { get; set; }
    public DateTime Timestamp { get; set; }
}

// Records (preferred for immutable data)
public record UserProfile(string Name, string Email, DateTime LastSeen);

// Usage
Task MessageReceived(ChatMessage message);
Task UserUpdated(UserProfile profile);
```

### Requirements for Complex Types

Complex types must be JSON serializable:

1. **Public parameterless constructor** (for classes)
2. **Record types** (automatically serializable)
3. **All properties must be serializable**
4. **Use `JsonConstructor` attribute** if needed

:::warning ⚠️ Unsupported Types
- Methods with `ref` or `out` parameters
- Generic method definitions
- Types with circular references
- Non-serializable types (like `Stream`, `Thread`)
:::

## Naming Conventions

### Interface Naming

```csharp
// ✅ Good
public interface IChatHubContract { }
public interface INotificationHubContract { }
public interface IGameHubContract { }

// ❌ Avoid
public interface ChatHub { }           // Missing 'I' prefix
public interface IHub { }              // Too generic
public interface IChatClient { }    // Redundant 'Client'
```

### Method Naming

```csharp
// ✅ Good - Server-to-Client
Task MessageReceived(string message);
Task UserJoined(string username);
Task GameStarted(int gameId);

// ✅ Good - Client-to-Server
Task SendMessage(string message);
Task JoinGame(int gameId);
Task<GameState> GetGameState(int gameId);

// ❌ Avoid
Task OnMessageReceived(string message);  // Don't use 'On' prefix
Task InvokeSendMessage(string message);  // Don't use 'Invoke' prefix
Task message_received(string message);   // Use PascalCase
```

## Complete Example

Here's a comprehensive chat hub example:

```csharp
using SignalRGen.Generator;

namespace ChatApp.Contracts;

[HubClient(HubUri = "chat")]
public interface IChatHubContract : IBidirectionalHub<IChatHubServerToClient, IChatHubClientToServer>
{
}

// Server-to-client communication
public interface IChatHubServerToClient
{
    Task UserJoined(string username);
    Task UserLeft(string username);
    Task MessageReceived(ChatMessage message);
    Task UserTyping(string username);
    Task UserStoppedTyping(string username);
    Task RoomUserCountUpdated(int count);
}

// Client-to-server communication
public interface IChatHubClientToServer
{
    Task SendMessage(string message);
    Task StartTyping();
    Task StopTyping();
    Task<List<string>> GetActiveUsers();
    Task<List<ChatMessage>> GetRecentMessages(int count);
}

// Supporting data types
public record ChatMessage(
    string Content,
    string Sender,
    DateTime Timestamp,
    string? ReplyToMessageId = null
);
```

## Multiple Hub Interfaces

You can define multiple hub interfaces in your application:

```csharp
[HubClient(HubUri = "chat")]
public interface IChatHubContract : IBidirectionalHub<IChatServerToClient, IChatClientToServer> { }

[HubClient(HubUri = "notifications")]
public interface INotificationHubContract : IServerToClientHub<INotificationServerToClient> { }

[HubClient(HubUri = "telemetry")]
public interface ITelemetryHubContract : IClientToServerHub<ITelemetryClientToServer> { }
```

Registration:
```csharp
services.AddSignalRHubs(options => 
{
    options.HubBaseUri = new Uri("https://api.example.com");
})
.WithChatHubContractClient()
.WithNotificationHubContractClient()
.WithTelemetryHubContractClient();
```
