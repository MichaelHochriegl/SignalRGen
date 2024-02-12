using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SignalRGen.Generator;

internal sealed record HubClientToGenerate(string InterfaceName, string HubName, string HubUri, IEnumerable<MethodDeclarationSyntax> Methods, IEnumerable<UsingDirectiveSyntax> Usings)
{
    public string InterfaceName { get; } = InterfaceName;
    public string HubName { get; } = HubName;
    public string HubUri { get; } = HubUri;
    public IEnumerable<MethodDeclarationSyntax> Methods { get; } = Methods;
    public IEnumerable<UsingDirectiveSyntax> Usings { get; } = Usings;
}

