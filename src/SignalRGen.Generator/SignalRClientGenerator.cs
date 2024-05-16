using System.Collections.Immutable;
using System.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SignalRGen.Generator.Common;
using SignalRGen.Generator.Sources;

namespace SignalRGen.Generator;

[Generator]
internal sealed class SignalRClientGenerator : IIncrementalGenerator
{
    private const string MarkerAttributeFullQualifiedName = "SignalRGen.Generator.HubClientAttribute";
 
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        Debugger.Launch();
        context.RegisterPostInitializationOutput(ctx => ctx.AddSource("HubClientAttribute.g.cs", HubClientAttributeSource.GetSource()));
        context.RegisterPostInitializationOutput(ctx => ctx.AddSource("HubClientBase.g.cs", HubClientBaseSource.GetSource()));
        context.RegisterPostInitializationOutput(ctx => ctx.AddSource("HubClientOptions.g.cs", HubClientOptionsSource.GetSource()));
        context.RegisterPostInitializationOutput(ctx => ctx.AddSource("IHubClient.g.cs", IHubClientSource.GetSource()));
        context.RegisterPostInitializationOutput(ctx => ctx.AddSource("SignalRHubServiceCollection.g.cs", FmSignalRHubServiceCollectionSource.GetSource()));
        context.RegisterPostInitializationOutput(ctx => ctx.AddSource("SignalROptions.g.cs", SignalROptionsSource.GetSource()));

        var markedInterfaces = context.SyntaxProvider.ForAttributeWithMetadataName(
            MarkerAttributeFullQualifiedName, static (syntaxNode, _) =>
                syntaxNode is InterfaceDeclarationSyntax { AttributeLists.Count: > 0 }, GetSemanticTargetForGeneration);
        var allHubClients = markedInterfaces.Collect();

        context.RegisterSourceOutput(markedInterfaces, GenerateHubClient!);
        context.RegisterSourceOutput(allHubClients, GenerateHubClientRegistration!);
    }
    
    private static void GenerateHubClient(SourceProductionContext context, HubClientToGenerate hubClientToGenerate)
    {
        context.AddSource($"{hubClientToGenerate.HubName}.g.cs", HubClientSource.GetSourceText(hubClientToGenerate));
    }
    
    private static void GenerateHubClientRegistration(SourceProductionContext context, ImmutableArray<HubClientToGenerate> hubClients)
    {
        context.AddSource("SignalRClientServiceRegistration.g.cs", SignalRClientServiceRegistrationSource.GetSource(hubClients));
    }

    private static HubClientToGenerate? GetSemanticTargetForGeneration(GeneratorAttributeSyntaxContext context, CancellationToken cancellationToken)
    {
        var node = (context.TargetNode as InterfaceDeclarationSyntax)!;

        var markerAttribute =
            context.SemanticModel.Compilation.GetTypeByMetadataName(MarkerAttributeFullQualifiedName);

        if (markerAttribute is null)
        {
            return null;
        }

        var hubClientAttribute = context.Attributes.FirstOrDefault(x =>
            x.AttributeClass is not null 
            // && x.AttributeClass.Equals(markerAttribute, SymbolEqualityComparer.Default));
            && x.AttributeClass.ToString() == markerAttribute.ToString());

        var usings = GetInterfacesUsings(node);
        var methods = GetInterfaceMethods(node);
        return hubClientAttribute is null ? null : new HubClientToGenerate(InterfaceName: node.Identifier.Text, HubName: GetHubNameOrDefaultConvention(hubClientAttribute, node), HubUri: GetHubUri(hubClientAttribute),
            InterfaceNamespace: GetInterfaceNamespace(node), Usings: usings, Methods: methods);
    }
    
    private static string GetHubUri(AttributeData hubClientAttribute)
    {
        var hubUri = hubClientAttribute.NamedArguments
            .FirstOrDefault(a => a.Key == "HubUri").Value.Value?
            .ToString();

        return hubUri ?? "Borked";
    }

    private static string GetInterfaceNamespace(InterfaceDeclarationSyntax interfaceDeclarationSyntax)
    {
        return interfaceDeclarationSyntax.FirstAncestorOrSelf<BaseNamespaceDeclarationSyntax>()?.Name.ToString() ?? "Borked";
    }

    private static EquatableArray<CacheableMethodDeclaration> GetInterfaceMethods(TypeDeclarationSyntax node)
    {
        // return node.Members.OfType<MethodDeclarationSyntax>();
        var methods = node.Members.OfType<MethodDeclarationSyntax>();
        return methods.Select(m => new CacheableMethodDeclaration(m.Identifier.Text,
             m.ParameterList.Parameters.Select(p => new Parameter(p.Type.ToString(), p.Identifier.Text)).ToImmutableArray().AsEquatableArray())).ToImmutableArray().AsEquatableArray();
    }

    private static EquatableArray<CacheableUsingDeclaration> GetInterfacesUsings(SyntaxNode syntaxNode)
    {
        // return syntaxNode.Parent?.Parent?.ChildNodes().OfType<UsingDirectiveSyntax>()
        //        ?? Enumerable.Empty<UsingDirectiveSyntax>();
        return syntaxNode.Parent?.Parent?.ChildNodes().OfType<UsingDirectiveSyntax>().Select(u => new CacheableUsingDeclaration(u.ToString())).ToImmutableArray().AsEquatableArray()
               ?? EquatableArray<CacheableUsingDeclaration>.FromImmutableArray(new ImmutableArray<CacheableUsingDeclaration>());
    }

    private static string GetHubNameOrDefaultConvention(AttributeData hubClientAttribute, InterfaceDeclarationSyntax syntaxNode)
    {
        var hubName = hubClientAttribute.NamedArguments.FirstOrDefault(n => n.Key == "HubName")
            .Value.Value;


        var rValue = hubName?.ToString() ?? syntaxNode.Identifier.Text.Substring(1);
        return rValue;
    }
}