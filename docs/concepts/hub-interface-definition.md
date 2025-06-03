# Hub Interface Definition

`SignalRGen` uses interfaces to define the contract between your client and server. This page explains how to create these interfaces and the attributes you'll use to control the generation process.

## The HubClient Attribute

Every SignalR interface must be marked with the `[HubClient]` attribute to be processed by the source generator:

::: code-group
```csharp
using SignalRGen.Generator;

namespace MyApp.Contracts;

[HubClient(HubUri = "example-hub")]
public interface IExampleHubClient
{
    // Methods will be defined here
}
```
:::

### HubClient Attribute Properties

The `HubClientAttribute` supports the following properties:

| Property | Type | Required | Description |
|----------|:----:|:--------:|-------------|
| `HubUri` | `string` | ✅ | The relative path where the hub is hosted on the server. This will be combined with the `HubBaseUri` configured globally. |
| `HubName` | `string` | ❌ | Optional custom name for the generated client class. If not specified, the interface name without the "I" prefix will be used. |

### Examples

```csharp
// Basic usage - generated class will be named "ExampleHubClient"
[HubClient(HubUri = "example")]
public interface IExampleHubClient { }

// With custom name - generated class will be named "CustomNamedClient"
[HubClient(HubUri = "custom-path", HubName = "CustomNamedClient")]
public interface IMyInterface { }
```

### URI Construction

The full hub URI is constructed by combining:
1. The global `HubBaseUri` from your configuration (e.g., `https://api.example.com`)
2. The `HubUri` from your interface attribute (e.g., `chat`)

For example:
- `HubBaseUri = "https://api.example.com"`
- `HubUri = "chat"`
- Final URI = `https://api.example.com/chat`

## Defining Methods

SignalRGen supports two types of methods in your interface:

1. **Server-to-Client Methods**: Methods that the server calls on the client
2. **Client-to-Server Methods**: Methods that the client calls on the server

### Server-to-Client Methods

By default, any method in your interface without an attribute is considered a server-to-client method:

```csharp
// This is a server-to-client method - the server will call this on the client
Task ReceiveMessage(string message, string sender);

// You can also be explicit with the optional attribute
[ServerToClientMethod]
Task UserJoined(string username);
```

Server-to-client methods:
- Must return `Task` or `Task<T>` (though `T` is rarely used for callbacks)
- Can have any number and type of parameters
- Do not need any special attributes (though you can use `[ServerToClientMethod]` for clarity)
- Will be exposed as events in the generated client

:::warning ⚠️ `CancellationToken` as Parameter
You don't have to include the `CancellationToken` in the parameters, `SignalRGen` adds those to the generated client automatically.
:::

### Client-to-Server Methods

Methods that the client calls on the server must be marked with the `[ClientToServerMethod]` attribute:

```csharp
// This is a client-to-server method - the client will call this on the server
[ClientToServerMethod]
Task SendMessage(string message);

// Client-to-server methods can return values from the server
[ClientToServerMethod]
Task<List<string>> GetActiveUsers();
```

Client-to-server methods:
- Must be marked with the `[ClientToServerMethod]` attribute
- Must return `Task` (no result) or `Task<T>` (for methods that return a value)
- Can have any number and type of parameters
- Will be exposed as methods in the generated client

:::warning ⚠️ `CancellationToken` as Parameter
You don't have to include the `CancellationToken` in the parameters, `SignalRGen` adds those to the generated client automatically.
:::

## Parameter and Return Types

SignalRGen supports a wide range of parameter and return types:

### Simple Types

```csharp
// Primitive types
Task ReceiveCount(int count);
Task ReceiveMessage(string message);
Task ReceiveFlag(bool isActive);

// Date and time
Task ReceiveTimestamp(DateTime timestamp);

// Nullable types
Task ReceiveNullableValue(int? maybeValue);
```

### Complex Types

```csharp
// Custom classes
Task ReceiveUser(User user);

// Records
Task ReceivePoint(Point point);

// Collections
Task ReceiveList(List<string> items);
Task ReceiveArray(string[] items);
Task ReceiveDictionary(Dictionary<string, int> scores);
```

### Requirements for Complex Types

Complex types must be serializable by the JSON serializer used by SignalR. This means:

1. They must have a public parameterless constructor, or
2. Be a record type, or
3. Have all properties serializable and have proper JsonConstructor attributes

## Naming Conventions

Following proper naming conventions makes your code more maintainable and easier to understand:

### Interface Naming

Interfaces should:
- Start with the "I" prefix
- End with the "HubClient" suffix (recommended but not required)
- Use PascalCase

Examples:
```csharp
public interface IChatHubClient { }
public interface INotificationHubClient { }
```

### Method Naming

Methods should:
- Use PascalCase
- Use verb-noun format for clarity
- Avoid prefixes like "On", "Invoke", etc. (`SignalRGen` adds these in the generated code)

Good examples:
```csharp
// Server-to-client methods
Task ReceiveMessage(string message);
Task UserJoined(string username);

// Client-to-server methods
[ClientToServerMethod]
Task SendMessage(string message);

[ClientToServerMethod]
Task<List<string>> GetActiveUsers();
```

Poor examples:
```csharp
// Don't use "On" prefix (SignalRGen adds this)
Task OnReceiveMessage(string message);

// Don't use "Invoke" prefix (SignalRGen adds this)
[ClientToServerMethod]
Task InvokeSendMessage(string message);
```

## Complete Example

Here's a complete example of a well-designed hub interface:

```csharp
using SignalRGen.Generator;

namespace ChatApp.Contracts;

[HubClient(HubUri = "chat")]
public interface IChatHubClient
{
    // Server-to-client methods
    Task ReceiveMessage(string message, string sender, DateTime timestamp);
    Task UserJoined(string username);
    Task UserLeft(string username);
    Task ReceiveTypingIndicator(string username, bool isTyping);
    
    // Client-to-server methods
    [ClientToServerMethod]
    Task SendMessage(string message);
    
    [ClientToServerMethod]
    Task SetTypingIndicator(bool isTyping);
    
    [ClientToServerMethod]
    Task<List<string>> GetActiveUsers();
    
    [ClientToServerMethod]
    Task<MessageHistory> GetMessageHistory(int count);
}

public record MessageHistory(List<ChatMessage> Messages, bool HasMoreMessages);
public record ChatMessage(string Content, string Sender, DateTime Timestamp);
```

## Best Practices

### Interface Design

1. **Keep interfaces focused**:
    - Create separate interfaces for different functional areas
    - Don't create catch-all interfaces with too many responsibilities

2. **Be explicit**:
    - Use descriptive method names
    - Consider using the `[ServerToClientMethod]` attribute for clarity, even though it's not required

3. **Version compatibility**:
    - Be careful when changing existing methods, as this affects the contract between client and server
    - Consider adding new methods rather than changing existing ones

### Method Parameters

1. **Use meaningful parameter names**:
    - Choose descriptive names that make the purpose clear
    - Don't use single-letter names like `x`, `y` unless appropriate (e.g., coordinates)

2. **Limit the number of parameters**:
    - Methods with many parameters are hard to use
    - Consider using parameter objects for methods with more than 3-4 parameters

3. **Use immutable types when possible**:
    - Records or read-only classes are preferable for complex data

## Advanced Scenarios

### Multiple Hub Interfaces

You can define multiple hub interfaces in your application:

```csharp
[HubClient(HubUri = "chat")]
public interface IChatHubClient { /* ... */ }

[HubClient(HubUri = "notifications")]
public interface INotificationHubClient { /* ... */ }
```

Each will generate its own client class, and you can register them all:

```csharp
services.AddSignalRHubs(options => 
{
    options.HubBaseUri = new Uri("https://api.example.com");
})
.WithChatHubClient()
.WithNotificationHubClient();
```

### Hub Interface Inheritance

:::danger ❗ NOT YET SUPPORTED
Hub interface inheritance is currently not supported, this is scheduled to be shipped with `v1.1.0`.
:::

## Server-Side Implementation

On the server side, you'll implement these interfaces in your Hub classes:

```csharp
// Server implementation
public class ChatHub : Hub<IChatHubClient>, IChatHubClient
{
    // Implement client-to-server methods
    public Task SendMessage(string message)
    {
        // Process the message
        
        // Call back to clients
        return Clients.All.ReceiveMessage(message, Context.ConnectionId, DateTime.UtcNow);
    }
    
    public Task<List<string>> GetActiveUsers()
    {
        // Return active users
        return Task.FromResult(new List<string> { "User1", "User2" });
    }
    
    // Server-to-client methods don't need implementation
    public Task ReceiveMessage(string message, string sender, DateTime timestamp)
    {
        throw new NotImplementedException("This is a server-to-client method");
    }
    
    public Task UserJoined(string username)
    {
        throw new NotImplementedException("This is a server-to-client method");
    }
    
    // Other methods...
}
```

For registration, you can access the generated URI with:
```csharp
app.MapHub<ChatHubClient>($"/{ChatHubClient.HubUri}");
```