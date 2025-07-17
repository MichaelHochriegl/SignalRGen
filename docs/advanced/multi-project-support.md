# Multi-Project Support

When using `SignalRGen` across multiple projects in the same solution, you may encounter naming conflicts with the 
generated extension methods. By default, SignalRGen generates an `AddSignalRHubs` extension method for 
dependency injection registration. If multiple projects use `SignalRGen`, this can lead to ambiguous method calls.

## The Problem

Consider a solution with two projects that both use SignalRGen:

```
MyApp.sln
├── MyApp.Chat/          (uses `SignalRGen` for chat functionality)
├── MyApp.Notifications/ (uses `SignalRGen` for notifications)
└── MyApp.Web/           (references both projects)
```

Both projects would generate the same `AddSignalRHubs` extension method, causing compilation errors in `MyApp.Web` 
when trying to register both sets of hub clients.

## Solution: Custom Module Names

`SignalRGen` provides the `SignalRModuleName` MSBuild property to customize the generated extension method name,
allowing you to avoid naming conflicts.

### Setting the Module Name

Add the `SignalRModuleName` property to your project file:

```csharp
    <Project Sdk="Microsoft.NET.Sdk">
      <PropertyGroup>
        <TargetFramework>net9.0</TargetFramework>
        <SignalRModuleName>Chat</SignalRModuleName>
      </PropertyGroup>

      // This here is important to expose the property to the source generator
      <ItemGroup>
        <CompilerVisibleProperty Include="SignalRModuleName" />
      </ItemGroup>
      
      <!-- Your SignalRGen package references -->
    </Project>
```

:::warning ⚠️ Important
Remember to include the `CompilerVisibleProperty` item - this makes the property available to the source generator.
:::

### Generated Extension Method

With `SignalRModuleName` set to "Chat", `SignalRGen` will generate:

```csharp
    public static SignalRHubServiceCollection<T> AddChatHubs(this IServiceCollection services, Action<SignalROptions> generalConfiguration)
    {
        // Implementation...
    }
```

Instead of the default `AddSignalRHubs`.

## Complete Example

Here's how you would set up two projects with different module names:

### Chat Project (MyApp.Chat.csproj)
```csharp
    <Project Sdk="Microsoft.NET.Sdk">
      <PropertyGroup>
        <TargetFramework>net9.0</TargetFramework>
        <SignalRModuleName>Chat</SignalRModuleName>
      </PropertyGroup>
      
      <ItemGroup>
        <CompilerVisibleProperty Include="SignalRModuleName" />
      </ItemGroup>
      
      <PackageReference Include="SignalRGen" Version="x.y.z">
        <PrivateAssets>all</PrivateAssets>
        <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
      </PackageReference>
    </Project>
```

### Notifications Project (MyApp.Notifications.csproj)
```csharp
    <Project Sdk="Microsoft.NET.Sdk">
      <PropertyGroup>
        <TargetFramework>net9.0</TargetFramework>
        <SignalRModuleName>Notifications</SignalRModuleName>
      </PropertyGroup>
      
      <ItemGroup>
        <CompilerVisibleProperty Include="SignalRModuleName" />
      </ItemGroup>
      
      <PackageReference Include="SignalRGen" Version="x.y.z">
        <PrivateAssets>all</PrivateAssets>
        <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
      </PackageReference>
    </Project>
```

### Registration in Main Application

```csharp
    // Program.cs in MyApp.Web
    var builder = WebApplication.CreateBuilder(args);

    // Register chat hub clients
    builder.Services
        .AddChatHubs(c => c.HubBaseUri = new Uri("http://localhost:5155"))
        .WithChatHubClient();

    // Register notification hub clients  
    builder.Services
        .AddNotificationsHubs(c => c.HubBaseUri = new Uri("http://localhost:5155"))
        .WithNotificationHubClient();

    var app = builder.Build();
```

## Default Behavior

If you don't specify a `SignalRModuleName`, `SignalRGen` defaults to "SignalR", generating the `AddSignalRHubs` extension method.
This is fine for single-project scenarios but should be customized for multi-project solutions.