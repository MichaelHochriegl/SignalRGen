namespace SignalRGen.Abstractions.Attributes;

/// <summary>
/// An attribute used to designate a method as a client-to-server method in the context of SignalR communications.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public sealed class ClientToServerMethodAttribute : Attribute
{
}