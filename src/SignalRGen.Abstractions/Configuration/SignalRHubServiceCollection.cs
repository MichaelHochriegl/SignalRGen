using Microsoft.Extensions.DependencyInjection;

namespace SignalRGen.Abstractions.Configuration;

/// <summary>
/// Represents a collection of services and configurations used for configuring SignalR hubs.
/// </summary>
/// <remarks>
/// This record encapsulates both the dependency injection service collection and general SignalR configuration options.
/// </remarks>
public record SignalRHubServiceCollection(IServiceCollection Services, SignalROptions GeneralConfiguration);