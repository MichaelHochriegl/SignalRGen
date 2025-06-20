namespace SignalRGen.Abstractions;

/// <summary>
/// Defines a contract for a hub client with a static abstract property to represent the hub's URI.
/// </summary>
public interface IHubClient
{
    /// <summary>
    /// Represents the URI of the hub for the implementing client.
    /// </summary>
    static abstract string HubUri { get; }
}