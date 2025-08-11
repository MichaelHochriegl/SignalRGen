# Hub Contract Interface Inheritance

`SignalRGen` supports composing your server-to-client (TServer) and client-to-server (TClient) contracts 
using C# interface inheritance. This lets you split large contracts into smaller, 
reusable pieces and assemble them per hub.

:::tip 📖 TL;DR: How it works
`SignalRGen` flattens the full interface hierarchy of `TServer` and `TClient` and treats all inherited methods
as part of the hub contract.
:::

## Why use inheritance?

- Reuse common operations across hubs
- Keep contracts small and focused
- Add features without breaking existing hubs
- Version contracts incrementally

## Quick examples

### Server-to-client: compose events from multiple bases

```csharp
public interface ICommonPresenceEvents
{
    Task UserJoined(string user);
    Task UserLeft(string user);
}

public interface IModerationEvents
{
    Task UserMuted(string user);
    Task UserUnmuted(string user);
}

public interface IChatHubServerToClient : ICommonPresenceEvents, IModerationEvents
{
    Task MessageReceived(ChatMessage message);
}
```

### Client-to-server: compose commands from multiple bases

```csharp
public interface IMessageCommands
{
    Task SendMessage(string message);
}

public interface IModerationCommands
{
    Task MuteUser(string user);
    Task UnmuteUser(string user);
}

public interface IChatHubClientToServer : IMessageCommands, IModerationCommands
{
    Task<List<string>> GetActiveUsers();
}
```

Use them in your hub contract as usual:

```csharp
using SignalRGen.Generator;

[HubClient(HubUri = "chat")]
public interface IChatHubContract : IBidirectionalHub<IChatHubServerToClient, IChatHubClientToServer>
{
    // No methods here and no inheritance here
}
```

## How inheritance is processed

- Flattening: `SignalRGen` collects methods from the interface and all its base interfaces (including multiple levels).
- Multiple inheritance: You can inherit from any number of base interfaces.
- Directional separation: `TServer` and `TClient` are processed independently; name collisions between them are fine.

## Rules and constraints

- Method rules are unchanged:
  - Server-to-client (`TServer`): methods must return `Task`
  - Client-to-server (`TClient`): methods must return `Task` or `Task<TResult>`
  - Any number of parameters; `CancellationToken` is injected automatically, do not include it
- Duplicates:
  - Allowed if signatures are identical (same name, parameters, and return type)
  - Conflicting signatures with the same name produce a broken client (see issue [#67](https://github.com/MichaelHochriegl/SignalRGen/issues/67))
- Ignored members:
  - Properties, events, indexers, static members, and default interface method bodies are ignored
- Accessibility:
  - Base interfaces must be accessible to the project using `SignalRGen`
- Cycles:
  - Circular inheritance is not allowed and results in a broken client
- Generics:
  - Generic base interfaces are supported as long as they are closed where used
  - Example: `ICrudCommands<OrderDto>` is fine; open generic `ICrudCommands<T>` must be closed in the derived interface


## Examples

### Multi-level inheritance

```csharp
public interface IBasePresenceEvents
{
    Task UserJoined(string user);
}

public interface IExtendedPresenceEvents : IBasePresenceEvents
{
    Task UserLeft(string user);
}

public interface INewsFeedEvents
{
    Task NewsPosted(string title, string content);
}

// SignalRGen will include all three events below
public interface IPortalServerToClient : IExtendedPresenceEvents, INewsFeedEvents
{
    Task SystemMessage(string message);
}
```

### Generic base interfaces

```csharp
public interface ICrudCommands<TDto>
{
    Task Create(TDto dto);
    Task<TDto> GetById(string id);
    Task Update(string id, TDto dto);
    Task Delete(string id);
}

public record OrderDto(string Id, string Number);

public interface IOrdersClientToServer : ICrudCommands<OrderDto>
{
    Task<List<OrderDto>> ListRecent(int count);
}
```

### Versioning via inheritance

```csharp
public interface IChatCommandsV1
{
    Task SendMessage(string message);
}

public interface IChatCommandsV2 : IChatCommandsV1
{
    Task EditMessage(string messageId, string newContent);
    Task DeleteMessage(string messageId);
}

public interface IChatHubClientToServer : IChatCommandsV2
{
    Task<List<string>> GetActiveUsers();
}
```
