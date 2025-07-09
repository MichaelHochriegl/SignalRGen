# Per-Hub configuration

Configure individual SignalR hubs with specific settings that apply only to that particular hub connection.

## Setup

Register hub-specific configurations after setting up global configuration:

::: code-group

```csharp [Per-Hub config example]
builder.Services.AddSignalRHubs(c => c.HubBaseUri = new Uri("http://localhost:5160"))
    // This is the bit that allows you to configure the hub client.
    .WithPingPongHub(c =>
    {
        // With this you have access to everything that `HttpConnectionOptions` allows to define.
        c.HttpConnectionOptionsConfiguration = options =>
        {
            options.Headers.Add("Authorization", "Bearer ");
        };
        
        // With this you have access to everything that `HubConnectionBuilder` allows to define.
        c.HubConnectionBuilderConfiguration = connectionBuilder =>
        {
            connectionBuilder.WithStatefulReconnect();
        };
        
        // With this you can define the lifetime of the hub client based on the `ServiceLifetime` enum.
        c.HubClientLifetime = ServiceLifetime.Scoped;
    });
```

:::

In the example above we define a couple of example settings.

## Available Options

The `HubClientOptions` class provides the following configuration options:

### `HubConnectionBuilderConfiguration`

Allows direct access to configure the underlying `HubConnectionBuilder` that builds the SignalR connection.

| Property | Type | Required | Default |
|----------|:----:|:--------:|---------|
| `HubConnectionBuilderConfiguration` | `Action<IHubConnectionBuilder>?` | ❌ | `null` |

**Common usages**:
- Setting authentication providers
- Configuring logging
- Implementing custom reconnection policies
- Setting message size limits

### `HttpConnectionOptionsConfiguration`

Provides configuration for the HTTP connection used by SignalR.

| Property | Type | Required | Default |
|----------|:----:|:--------:|---------|
| `HttpConnectionOptionsConfiguration` | `Action<HttpConnectionOptions>?` | ❌ | `null` |

**Common usages**:
- Adding custom, static headers
- Setting transport types (WebSockets, ServerSentEvents, etc.)
- Configuring proxy settings
- Setting connection timeouts

### `HubClientLifetime`

Controls the DI lifetime of the hub client instance.

| Property | Type | Required | Default |
|----------|:----:|:--------:|---------|
| `HubClientLifetime` | `ServiceLifetime` | ❌ | `ServiceLifetime.Singleton` |

**Available options**:
- `ServiceLifetime.Singleton` - One instance shared across the application (default)
- `ServiceLifetime.Scoped` - New instance per scope (e.g., per request in ASP.NET)
- `ServiceLifetime.Transient` - New instance each time it's requested

## Default Behavior

When no configuration is provided, SignalRGen sets up sensible defaults:

- The hub client is registered as a `Singleton`
- A default retry policy is applied with:
    - 10 attempts at 1-second intervals
    - 5 attempts at 3-second intervals
    - 2 attempts at 10-second intervals
- The hub URI is constructed by combining the global `HubBaseUri` (see [global configuration](config-global.md#hubbaseuri))
with the hub-specific `HubUri`
