//HintName: HubClientAttribute.g.cs
namespace SignalRGen.Generator;
[System.AttributeUsage(System.AttributeTargets.Interface)]
public sealed class HubClientAttribute : System.Attribute
{
    public string? HubName;
}