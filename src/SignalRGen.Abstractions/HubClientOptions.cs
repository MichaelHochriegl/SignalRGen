using Microsoft.AspNetCore.Http.Connections.Client;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;

namespace SignalRGen.Abstractions;

/// <summary>
/// Encapsulates the options to configure a SignalR Hub Client.
/// </summary>
public class HubClientOptions
{
    /// <summary>The configuration used to set up the <see cref = "HubConnectionBuilder"/> that will be used to build to the <see cref = "HubConnection"/>.</summary>
    public Action<IHubConnectionBuilder>? HubConnectionBuilderConfiguration { get; set; }

    /// <summary>The configuration used to set up the <see cref="HttpConnectionOptions"/> for constructing the SignalR Hub connection.</summary>
    public Action<HttpConnectionOptions>? HttpConnectionOptionsConfiguration { get; set; }
    
    /// <summary>The <see cref = "ServiceLifetime"/> used to register the Hub Client with the DI container.</summary>
    public ServiceLifetime HubClientLifetime { get; set; } = ServiceLifetime.Singleton;
}