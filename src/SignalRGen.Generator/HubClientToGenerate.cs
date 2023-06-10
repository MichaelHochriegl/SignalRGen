using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SignalRGen.Generator;

public class HubClientToGenerate
{
    public HubClientToGenerate(string hubName, IEnumerable<MethodDeclarationSyntax> methods, IEnumerable<UsingDirectiveSyntax> usings)
    {
        HubName = hubName;
        Methods = methods;
        Usings = usings;
    }

    public string HubName { get; }
    public IEnumerable<MethodDeclarationSyntax> Methods { get; }
    public IEnumerable<UsingDirectiveSyntax> Usings { get; }
}