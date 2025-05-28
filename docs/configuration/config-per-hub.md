# Per-Hub configuration

This configuration can be done for each `Hub` differently.

To set it up, you use the DI registration:

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

## Configuration values

### `HubConnectionBuilderConfiguration`

Allows you to configure the `HubConnectionBuilder` that is used to build the `Hub`.

| Name                                 |               Type               | Default Value                                                                    |
|--------------------------------------|:--------------------------------:|----------------------------------------------------------------------------------|
| `HubConnectionBuilderConfiguration` | `Action<IHubConnectionBuilder>?` | `null`, so the default values will be used, as described in the MS documentation |


### `HttpConnectionOptionsConfiguration`

Allows you to configure the underlying `HttpConnection` of the `Hub`.

| Name                                 |               Type               | Default Value                                                                    |
|--------------------------------------|:--------------------------------:|----------------------------------------------------------------------------------|
| `HttpConnectionOptionsConfiguration` | `Action<HttpConnectionOptions>?` | `null`, so the default values will be used, as described in the MS documentation |

### `HubClientLifetime`

Allows you to configure what lifetime is used for the `Hub`.

| Name                |       Type        | Default Value                                                                          |
|---------------------|:-----------------:|----------------------------------------------------------------------------------------|
| `HubClientLifetime` | `ServiceLifetime` | `ServiceLifetime.Singleton`, so only one instance is created and shared across the app |
