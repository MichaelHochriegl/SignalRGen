# Global configuration

This configuration can be used to set up global values used by all your `Hubs`.

To set it up, you use the DI registration:

::: code-group

```csharp [Global config example]
builder.Services
    .AddSignalRHubs(c => c.HubBaseUri = new Uri("http://localhost:5160"));
```

:::

In the example above we define the global URI from the server we want to talk to.

## Configuration values

### `HubBaseUri`

Allows you to define the server URI all the Hubs will be talking too.

| Name         | Type  | Required? | Default Value |
|--------------|:-----:|-----------|---------------|
| `HubBaseUri` | `Uri` |  ✅         | `null`        |