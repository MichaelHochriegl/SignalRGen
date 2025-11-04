using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SignalRGen.Testing.Generator.Extractors;
using SignalRGen.Testing.Generator.Sources;

namespace SignalRGen.Testing.Generator;

[Generator]
internal sealed class FakeSignalRClientGenerator : IIncrementalGenerator
{
    private const string MarkerAttributeFullQualifiedName = "SignalRGen.Testing.Abstractions.Attributes.GenerateFakeForHubClientAttribute";
    
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var markedTypes = context.SyntaxProvider.ForAttributeWithMetadataName(
            MarkerAttributeFullQualifiedName,
            static (syntaxNode, _) => syntaxNode is CompilationUnitSyntax,
            GetSemanticTargetForGeneration)
            .WithTrackingName(TrackingNames.InitialExtraction);
        
        context.RegisterSourceOutput(markedTypes, GenerateFakeHubClient);
    }

    private void GenerateFakeHubClient(SourceProductionContext context, FakeHubClientToGenerate? fakeHubClientToGenerate)
    {
        if (fakeHubClientToGenerate is null) return;
        var sourceText = FakeHubClientSource.GetSource(fakeHubClientToGenerate);
        context.AddSource($"Fake{fakeHubClientToGenerate.HubClientName}.g.cs", sourceText);
    }

    private static FakeHubClientToGenerate? GetSemanticTargetForGeneration(
        GeneratorAttributeSyntaxContext context,
        CancellationToken cancellationToken)
    {
        var markerAttribute =
            context.SemanticModel.Compilation.GetTypeByMetadataName(MarkerAttributeFullQualifiedName);

        if (markerAttribute is null)
        {
            return null;
        }

        var fakeHubClientAttribute = context.Attributes.FirstOrDefault(x =>
            x.AttributeClass is not null
            && x.AttributeClass.Equals(markerAttribute, SymbolEqualityComparer.Default));

        if (fakeHubClientAttribute is null)
        {
            return null;
        }
        
        var hubClientAttributeArgument = fakeHubClientAttribute.ConstructorArguments.FirstOrDefault();

        if (hubClientAttributeArgument.Value is not INamedTypeSymbol hubClientTypeSymbol)
        {
            return null;            
        }
        var hubClientNamespace = hubClientTypeSymbol.ContainingNamespace.ToString();
        var hubClientName = hubClientTypeSymbol.Name;
        
        var extractor = new HubClientMethodExtractor(context.SemanticModel, hubClientTypeSymbol);
        var extractedData = extractor.Extract();
        
        return new FakeHubClientToGenerate(hubClientNamespace, hubClientName, extractedData.ClientToServerMethods, extractedData.ServerToClientMethods);
    }
}