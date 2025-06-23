namespace SignalRGen.Abstractions.Attributes;

/// <summary>
/// An attribute used to designate an interface as a SignalR client hub
/// and provide configuration details such as the hub URI and optional hub name.
/// </summary>
[AttributeUsage(AttributeTargets.Interface)]
public sealed class HubClientAttribute : Attribute
{
    /// <summary>
    /// Specifies the name of the SignalR hub.
    /// This property can be used to define an optional, custom name for the hub
    /// that differs from the default.
    /// </summary>
    public string? HubName { get; init; }

    /// <summary>
    /// Represents the relative URI of the SignalR hub.
    /// This property is required and specifies the relative address
    /// where the SignalR hub can be accessed.
    /// </summary>
    public required string HubUri { get; init; }
}