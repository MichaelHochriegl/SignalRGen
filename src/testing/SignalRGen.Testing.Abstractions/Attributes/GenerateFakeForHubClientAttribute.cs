namespace SignalRGen.Testing.Abstractions.Attributes;

/// <summary>
/// An attribute that specifies a SignalR Hub client interface type for which a fake implementation should be generated.
/// </summary>
/// <remarks>
/// This attribute is intended to be used at the assembly level to indicate that
/// a fake implementation of the given Hub client interface type should be created
/// for testing purposes.
/// </remarks>
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public sealed class GenerateFakeForHubClientAttribute : Attribute
{
    /// <summary>
    /// An attribute to generate a fake implementation for a specified SignalR hub client interface.
    /// </summary>
    /// <remarks>
    /// This attribute is applied at the assembly level and can be used multiple times within the same assembly.
    /// It specifies the type of the hub client interface for which a fake implementation should be generated.
    /// </remarks>
    public GenerateFakeForHubClientAttribute(Type hubClientInterfaceType)
    {
        HubClientInterfaceType = hubClientInterfaceType;
    }

    /// <summary>
    /// Gets the interface type representing the SignalR hub client.
    /// This property is used to signify the interface for which a fake
    /// implementation is generated at runtime for testing purposes.
    /// </summary>
    public Type HubClientInterfaceType { get; }
}