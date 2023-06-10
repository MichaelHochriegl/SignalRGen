using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SignalRGen.Generator.Sources;

namespace SignalRGen.Generator;

[Generator]
internal sealed class SignalRClientGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(ctx => ctx.AddSource("HubClientAttribute.g.cs", HubClientAttributeSource.GetSource()));
        
        var interfaces =
            context.SyntaxProvider.CreateSyntaxProvider(static (syntaxNode, _) =>
                    syntaxNode is InterfaceDeclarationSyntax { AttributeLists.Count: > 0 }, GetSemanticTargetForGeneration)
                .Where(n => n is not null);

        context.RegisterSourceOutput(interfaces, GenerateHubClient!);
    }
    
    private static void GenerateHubClient(SourceProductionContext context, HubClientToGenerate hubClientToGenerate)
    {
        context.AddSource($"{hubClientToGenerate.HubName}.g.cs", HubClientSource.GetSourceText(hubClientToGenerate));
    }

    #region Get Hub-Clients To Generate

    private static HubClientToGenerate? GetSemanticTargetForGeneration(GeneratorSyntaxContext context, CancellationToken cancellationToken)
    {
        var node = (context.Node as InterfaceDeclarationSyntax)!;
        var declaredSymbol = context.SemanticModel.GetDeclaredSymbol(node, cancellationToken);
        if (declaredSymbol is null)
            return null;

        var hubClientAttribute = declaredSymbol.GetAttributes().FirstOrDefault(x =>
            x.AttributeClass is not null && x.AttributeClass.ToString() == "HubClient");
        return hubClientAttribute is null ? null : new HubClientToGenerate(hubName: GetHubNameOrDefaultConvention(hubClientAttribute, node),
            usings: GetInterfacesUsings(node), methods: GetInterfaceMethods(node));
    }

    private static IEnumerable<MethodDeclarationSyntax> GetInterfaceMethods(TypeDeclarationSyntax node)
    {
        return node.Members.OfType<MethodDeclarationSyntax>();
    }

    private static IEnumerable<UsingDirectiveSyntax> GetInterfacesUsings(SyntaxNode syntaxNode)
    {
        return syntaxNode.Parent?.Parent?.ChildNodes().OfType<UsingDirectiveSyntax>()
               ?? Enumerable.Empty<UsingDirectiveSyntax>();
    }

    private static string GetHubNameOrDefaultConvention(AttributeData hubClientAttribute, InterfaceDeclarationSyntax syntaxNode)
    {
        var hubName = hubClientAttribute.NamedArguments.FirstOrDefault(n => n.Key == "HubName")
            .Value.Value;


        var rValue = hubName?.ToString() ?? syntaxNode.Identifier.Text.Substring(1);
        return rValue;
    }

    #endregion
}