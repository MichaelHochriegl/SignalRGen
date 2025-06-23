namespace SignalRGen.Abstractions.Attributes;

/// <summary>
/// An attribute used to designate a method as a server-to-client method in the context of SignalR communications.
/// </summary>
/// <remarks>
/// This attribute is optional, as SignalRGen will automatically assume that methods with no attribute are server-to-client methods.
/// </remarks>
[AttributeUsage(AttributeTargets.Method)]
public sealed class ServerToClientMethodAttribute : Attribute
{
}