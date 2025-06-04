# What is SignalRGen?

SignalRGen is a source generator library for .NET that transforms the way you work with SignalR, Microsoft's real-time communication framework. It eliminates boilerplate code and provides a strongly-typed, developer-friendly experience.

## The Problem SignalRGen Solves

Traditional SignalR client implementation requires:
- Writing repetitive connection management code
- Using string-based method names (prone to typos)
- Creating custom wrapper classes for type safety
- Implementing your own reconnection logic

This leads to verbose, error-prone code challenging to maintain as your application grows.

## How SignalRGen Works

SignalRGen takes a different approach:

1. **Define Once**: Create a simple interface that defines your SignalR contract
2. **Generate Automatically**: The source generator creates a complete client implementation
3. **Use Anywhere**: Register via dependency injection and use the strongly-typed client

No more string-based method names, manual connection handling, or tedious boilerplate!

## Key Features

- **Zero Runtime Dependencies**: All code is generated at build time
- **Strongly-Typed Communication**: Full IntelliSense support and compile-time safety
- **Modern .NET Integration**: Seamless dependency injection support
- **Automatic Reconnection**: Built-in resilient connection handling
- **Full Customization**: Configure every aspect of the SignalR connection when needed

## Example: The SignalRGen Difference

### Traditional SignalR Implementation

With traditional SignalR, you need to manage connections manually, use string-based method names, and implement your own type-safe wrappers:

```csharp
// Traditional approach - lots of boilerplate and string-based APIs
public class ChatClient : IAsyncDisposable
{
    private readonly HubConnection _connection;
    private bool _isConnected;
    
    public ChatClient(string baseUrl)
    {
        // Manually create and configure connection
        _connection = new HubConnectionBuilder()
            .WithUrl($"{baseUrl}/chathub")
            .WithAutomaticReconnect()
            .Build();
            
        // String-based method registration - prone to typos!
        _connection.On<string, string>("ReceiveMessage", HandleMessage);
        
        // Manual connection state management
        _connection.Closed += async (error) => {
            _isConnected = false;
            await ReconnectWithRetryAsync();
        };
    }
    
    // Event to expose the received messages
    public event Action<string, string> MessageReceived;
    
    private Task HandleMessage(string user, string message)
    {
        MessageReceived?.Invoke(user, message);
        return Task.CompletedTask;
    }
    
    // Complex connection management
    public async Task ConnectAsync()
    {
        if (_isConnected) return;
        
        await _connection.StartAsync();
        _isConnected = true;
    }
    
    // Custom reconnection logic
    private async Task ReconnectWithRetryAsync()
    {
        // Implement complex retry logic
        // ...
    }
    
    // String-based method invocation - no compile-time safety!
    public async Task SendMessageAsync(string message)
    {
        if (!_isConnected) await ConnectAsync();
        await _connection.InvokeAsync("SendMessage", message);
    }
    
    public async ValueTask DisposeAsync()
    {
        await _connection.DisposeAsync();
    }
}

// Usage requires manual instantiation and management
var client = new ChatClient("https://example.com");
await client.ConnectAsync();
client.MessageReceived += (user, message) => Console.WriteLine($"{user}: {message}");
await client.SendMessageAsync("Hello world");
```

### SignalRGen Implementation

With SignalRGen, you only need to define a simple interface, and everything else is generated for you:

```csharp
// 1. Define your hub interface - that's it!
[HubClient(HubUri = "chathub")]
public interface IChatHub
{
    // Server-to-client methods
    Task ReceiveMessage(string user, string message);
    
    // Client-to-server methods
    [ClientToServerMethod]
    Task SendMessage(string message);
}

// 2. Register in your DI container
services.AddSignalRHubs(c => c.HubBaseUri = new Uri("https://example.com"))
    .WithChatHub();

// 3. Use the generated client with full type safety
public class ChatService
{
    private readonly ChatHub _hub;
    
    // Just inject and use!
    public ChatService(ChatHub hub)
    {
        _hub = hub;
        
        // Strongly-typed event subscription
        _hub.OnReceiveMessage += (user, message) => {
            Console.WriteLine($"{user}: {message}");
            return Task.CompletedTask;
        };
    }
    
    // Connection management handled automatically
    public async Task SendMessageAsync(string message)
    {
        // Strongly-typed method invocation
        await _hub.InvokeSendMessageAsync(message);
    }
}
```

## Who Should Use SignalRGen?

- .NET developers building real-time applications with SignalR
- Teams seeking to reduce boilerplate code and improve maintainability
- Projects that need type-safe, reliable real-time communication
- Teams that want to easily package and share SignalR clients as NuGet packages across multiple projects
