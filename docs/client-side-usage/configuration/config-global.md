# Global configuration

Configure global settings that apply to the SignalR client across all Hubs.

## Usage

Register global configuration through dependency injection:

::: code-group

```csharp [Global config example]
builder.Services
    .AddSignalRHubs(c => c.HubBaseUri = new Uri("http://localhost:5160"));
```

:::


In the example above we define the global URI from the server we want to talk to.

:::tip 💡 Changing `AddSignalRHubs` naming
You can change the naming of the `AddSignalRHubs` method by supplying a `SignalRModuleName` in your `csproj`:

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

This also allows you to use the `AddSignalRHubs` method in multiple projects.
See [Multi-Project Support](../../advanced/multi-project-support) for more information.
:::


## Configuration values

### `HubBaseUri`

Defines the base URI used for all Hub connections.

| Name | Type | Required? | Default Value |
|------|:----:|-----------|---------------|
| `HubBaseUri` | `Uri` | ✅ | `null`, so you must provide a value unless overridden at the Hub level |

## Usage Notes

- Global settings apply to all Hubs and cannot be overriden at the Hub level, it's not a hierarchy,
    both configuration locations serve different purposes
- See Hub-specific configuration options on the Hub Configuration page
