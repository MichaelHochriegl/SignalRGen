//HintName: HubClientAttribute.g.cs
using System;

namespace SignalRGen.Generator;
/// <summary>
/// Marker Attribute for a HubClient.
/// </summary>
[AttributeUsage(AttributeTargets.Interface)]
public sealed class HubClientAttribute : Attribute
{
    public string? HubName { get; init; }
    public required string HubUri { get; init; }
}