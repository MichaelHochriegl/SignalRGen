using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SignalRGen.Generator.Common;
using SignalRGen.Generator.Extractors;
using SignalRGen.Generator.Sources;

namespace SignalRGen.Generator;

[Generator]
internal sealed class SignalRClientGenerator : IIncrementalGenerator
{
    private const string MarkerAttributeFullQualifiedName = "SignalRGen.Abstractions.Attributes.HubClientAttribute";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var msBuildOptions = context
            .AnalyzerConfigOptionsProvider
            .Select((c, _) =>
                c.GlobalOptions.TryGetValue("build_property.RootNamespace", out var rootNamespace)
                    ? new MsBuildOptions(rootNamespace)
                    : null);
        
        context.RegisterSourceOutput(msBuildOptions, (ctx, options) => 
            ctx.AddSource("HubClientBase.g.cs", HubClientBaseSource.GetSource(options))
            );

        var markedInterfaces = context.SyntaxProvider.ForAttributeWithMetadataName(
                MarkerAttributeFullQualifiedName, static (syntaxNode, _) =>
                    syntaxNode is InterfaceDeclarationSyntax { AttributeLists.Count: > 0 },
                GetSemanticTargetForGeneration)
            .WithTrackingName(TrackingNames.InitialExtraction);
        var allHubClients = markedInterfaces.Collect().WithTrackingName(TrackingNames.Collect);

        context.RegisterSourceOutput(markedInterfaces, GenerateHubClient!);
        context.RegisterSourceOutput(allHubClients, GenerateHubClientRegistration!);
    }

    private static void GenerateHubClient(SourceProductionContext context, HubClientToGenerate hubClientToGenerate)
    {
        context.AddSource($"{hubClientToGenerate.HubName}.g.cs", HubClientSource.GetSourceText(hubClientToGenerate));
    }

    private static void GenerateHubClientRegistration(SourceProductionContext context,
        ImmutableArray<HubClientToGenerate> hubClients)
    {
        context.AddSource("SignalRClientServiceRegistration.g.cs",
            SignalRClientServiceRegistrationSource.GetSource(hubClients));
    }

    private static HubClientToGenerate? GetSemanticTargetForGeneration(
        GeneratorAttributeSyntaxContext context,
        CancellationToken cancellationToken)
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
            && x.AttributeClass.Equals(markerAttribute, SymbolEqualityComparer.Default));

        if (hubClientAttribute is null)
        {
            return null;
        }

        // Get the interface symbol
        if (context.SemanticModel.GetDeclaredSymbol(node) is not INamedTypeSymbol interfaceSymbol)
        {
            return null;
        }

        // Extract all methods and usings from the interface and its base interfaces
        var extractor = new InterfaceMethodExtractor(
            context.SemanticModel,
            node,
            interfaceSymbol,
            cancellationToken);
    
        var extractedData = extractor.Extract();

        return new HubClientToGenerate(
            InterfaceName: node.Identifier.Text,
            HubName: GetHubNameOrDefaultConvention(hubClientAttribute, node),
            HubUri: GetHubUri(hubClientAttribute),
            InterfaceNamespace: GetInterfaceNamespace(context.TargetSymbol),
            Usings: extractedData.Usings,
            ServerToClientMethods: extractedData.ServerToClientMethods,
            ClientToServerMethods: extractedData.ClientToServerMethods);
    }

    private static string GetHubUri(AttributeData hubClientAttribute)
    {
        var hubUri = hubClientAttribute.NamedArguments
            .First(a => a.Key == "HubUri").Value.Value!
            .ToString();

        return hubUri;
    }
    
    private static string GetInterfaceNamespace(ISymbol interfaceSymbol)
    {
        return interfaceSymbol.ContainingNamespace.ToString();
    }

    private static EquatableArray<CacheableUsingDeclaration> GetInterfacesUsings(SyntaxNode syntaxNode)
    {
        return syntaxNode.Parent?.Parent?.ChildNodes().OfType<UsingDirectiveSyntax>()
                   .Select(u => new CacheableUsingDeclaration(u.ToString())).ToImmutableArray().AsEquatableArray()
               ?? EquatableArray<CacheableUsingDeclaration>.FromImmutableArray(
                   new ImmutableArray<CacheableUsingDeclaration>());
    }

    private static string GetHubNameOrDefaultConvention(AttributeData hubClientAttribute,
        InterfaceDeclarationSyntax syntaxNode)
    {
        var hubName = hubClientAttribute.NamedArguments.FirstOrDefault(n => n.Key == "HubName")
            .Value.Value;


        var rValue = hubName?.ToString() ?? $"{syntaxNode.Identifier.Text.Substring(1)}Client";
        return rValue;
    }
}