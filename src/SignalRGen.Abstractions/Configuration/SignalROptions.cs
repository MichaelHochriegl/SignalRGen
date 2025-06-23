namespace SignalRGen.Abstractions.Configuration;

/// <summary>
/// Encapsulates the options to configure a SignalR Client.
/// </summary>
public class SignalROptions
{
    /// <summary>The base <see cref="Uri"/> for the SignalR Client.</summary>
    /// <remarks>This <see cref="Uri"/> will be used for every `With...` Hub defined.</remarks>
    public Uri HubBaseUri { get; set; } = null!;
}