using Microsoft.CodeAnalysis.CSharp.Syntax;
using SignalRGen.Generator.Common;

namespace SignalRGen.Generator;

internal sealed record HubClientToGenerate(string InterfaceName, string HubName, string HubUri, EquatableArray<CacheableMethodDeclaration> Methods, IEnumerable<UsingDirectiveSyntax> Usings)
{
    public string InterfaceName { get; } = InterfaceName;
    public string HubName { get; } = HubName;
    public string HubUri { get; } = HubUri;
    public EquatableArray<CacheableMethodDeclaration> Methods { get; } = Methods;
    public IEnumerable<UsingDirectiveSyntax> Usings { get; } = Usings;
}

