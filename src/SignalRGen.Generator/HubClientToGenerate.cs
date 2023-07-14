using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SignalRGen.Generator;

internal sealed class HubClientToGenerate
{
    public readonly string InterfaceName;
    public readonly string HubName;
    public readonly string HubUri;
    public readonly IEnumerable<MethodDeclarationSyntax> Methods;
    public readonly IEnumerable<UsingDirectiveSyntax> Usings;

    public HubClientToGenerate(string interfaceName, string hubName, string hubUri,
        IEnumerable<MethodDeclarationSyntax> methods,
        IEnumerable<UsingDirectiveSyntax> usings)
    {
        HubName = hubName;
        HubUri = hubUri;
        Methods = methods;
        Usings = usings;
        InterfaceName = interfaceName;
    }

    private bool Equals(HubClientToGenerate other)
    {
        return InterfaceName == other.InterfaceName &&
               HubName == other.HubName && HubUri == other.HubUri &&
               Methods.SequenceEqual(other.Methods) &&
               Usings.SequenceEqual(other.Usings);
    }

    public override bool Equals(object? obj)
    {
        return ReferenceEquals(this, obj) || obj is HubClientToGenerate other && Equals(other);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            var hashCode = HubName.GetHashCode();
            hashCode = (hashCode * 397) ^ InterfaceName.GetHashCode();
            hashCode = (hashCode * 397) ^ HubUri.GetHashCode();
            hashCode = (hashCode * 397) ^ Methods.GetHashCode();
            hashCode = (hashCode * 397) ^ Usings.GetHashCode();
            return hashCode;
        }
    }
}