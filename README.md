[![Pipeline](https://github.com/MichaelHochriegl/SignalRGen/actions/workflows/ci.yml/badge.svg?branch=master)](https://github.com/MichaelHochriegl/SignalRGen/actions/workflows/ci.yml)
[![Static Badge](https://img.shields.io/badge/license-MIT-blue?style=flat&logo=refinedgithub&logoColor=white)](https://github.com/MichaelHochriegl/SignalRGen/blob/main/LICENSE)
[![NuGet](https://img.shields.io/nuget/v/SignalRGen)](https://www.nuget.org/packages/SignalRGen)

# SignalRGen ![Package-Logo](https://raw.githubusercontent.com/MichaelHochriegl/SignalRGen/refs/heads/main/assets/logo_32x32.png)

A source generator for SignalR that eliminates boilerplate and provides strongly-typed clients


## What is SignalRGen?

`SignalRGen` transforms the SignalR experience by using source generation to:

- ✅ **Eliminate boilerplate connection management code**
- ✅ **Provide strongly-typed method calls and event handlers**
- ✅ **Enable seamless dependency injection integration**
- ✅ **Allow easy distribution of clients as NuGet packages**

## Installation

```shell
dotnet add package SignalRGen
```

YOLO development builds are also available in this GitHub repository.

## Quick Start

### 1. Define your Hub interface

```csharp
using SignalRGen.Generator;

[HubClient(HubUri = "examples")]
public interface IExampleHubClient
{
    Task ReceiveExampleCountUpdate(int count);
    
    [ClientToServerMethod]
    Task SendMessage(string message);
}
```

### 2. Register on the server

```csharp
var app = builder.Build();

app.MapHub<ExampleHub>($"/{ExampleHubClient.HubUri}");
```

### 3. Register in the client

```csharp
builder.Services
    .AddSignalRHubs(c => c.HubBaseUri = new Uri("http://localhost:5155"))
    .WithExampleHubClient();
```

### 4. Use the generated client

```csharp
public class ExampleComponent
{
    private readonly ExampleHubClient _hubClient;
    
    public ExampleComponent(ExampleHubClient hubClient)
    {
        _hubClient = hubClient;
        _hubClient.OnReceiveExampleCountUpdate += HandleCountUpdate;
    }
    
    public async Task Initialize()
    {
        // Connection handling is done automatically
        await _hubClient.StartAsync();
        
        // Strongly-typed method invocation
        await _hubClient.InvokeSendMessageAsync("Hello from SignalRGen!");
    }
    
    private async Task HandleCountUpdate(int count)
    {
        Console.WriteLine($"New count: {count}");
        return Task.CompletedTask;
    }
}
```

## How It Works

`SignalRGen` uses C# source generators to analyze your SignalR hub interfaces and automatically generate:

1. **Strongly-typed hub clients** - No more string-based method names
2. **Connection management** - Automatic reconnection and state handling
3. **DI extensions** - Simple registration with your IoC container
4. **Event handlers** - Type-safe callbacks for server notifications

### Example Blazor Component

```csharp
@page "/"
@using SignalRGen.Generator

@inject ExampleHubClient ExampleHubClient;

<h1>Hello, world!</h1>

<p>Current count received via SignalR: @_count</p>

@code
{
    private int _count = 0;
    
    protected override async Task OnInitializedAsync()
    {
        ExampleHubClient.OnReceiveExampleCountUpdate += OnReceiveExampleCountUpdate;
        await ExampleHubClient.StartAsync();
    }

    private async Task OnReceiveExampleCountUpdate(int arg)
    {
        _count = arg;
        await InvokeAsync(StateHasChanged);
    }
}
```

## Advanced Configuration

`SignalRGen` allows full customization of your hub connections:

```csharp
builder.Services
    .AddSignalRHubs(c => c.HubBaseUri = new Uri("http://localhost:5155"))
    .WithExampleHubClient(options =>
    {
        // Change lifetime (Singleton is default)
        options.HubClientLifetime = ServiceLifetime.Scoped;
        
        // Configure the connection
        options.HubConnectionBuilderConfiguration = connectionBuilder =>
            connectionBuilder
                .WithAutomaticReconnect()
                .WithHubProtocol(new MessagePackHubProtocol());
    });
```

## Client-Server Integration

When you define methods with the `[ClientToServerMethod]` attribute, `SignalRGen` will generate strongly-typed invocation methods:

```csharp
// Your interface definition
[HubClient(HubUri = "chat")]
public interface IChatHub
{
    Task ReceiveMessage(string user, string message);
    
    [ClientToServerMethod]
    Task SendMessage(string message);
}

// Usage with generated client
await chatHub.InvokeSendMessageAsync("Hello world!");
```

## Documentation

For complete documentation, samples, and API reference, visit our [documentation site](https://signalrgen.net).

## Acknowledgements

This library wouldn't be possible without the following people:
* [Andrew Lock](https://andrewlock.net)
* [Anton Wieslander (aka RawCoding)](https://www.youtube.com/RawCoding)
* [Jeffrey T. Fritz (aka csharpfritz)](https://www.twitch.tv/csharpfritz)
* [BissauCam](https://github.com/BissauCam)
```