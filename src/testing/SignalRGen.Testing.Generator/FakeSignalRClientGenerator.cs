using System.Collections.Immutable;
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
            .SelectMany(static (items, _) => items)
            .WithTrackingName(TrackingNames.InitialExtraction);
   
        context.RegisterSourceOutput(markedTypes, GenerateFakeHubClient);
    }

    private void GenerateFakeHubClient(SourceProductionContext context, FakeHubClientToGenerate fakeHubClientToGenerate)
    {
        var sourceText = FakeHubClientSource.GetSource(fakeHubClientToGenerate);
        context.AddSource($"Fake{fakeHubClientToGenerate.HubClientName}.g.cs", sourceText);
    }

    private static ImmutableArray<FakeHubClientToGenerate> GetSemanticTargetForGeneration(
        GeneratorAttributeSyntaxContext context,
        CancellationToken cancellationToken)
    {
        var markerAttribute =
            context.SemanticModel.Compilation.GetTypeByMetadataName(MarkerAttributeFullQualifiedName);

        if (markerAttribute is null)
        {
            return ImmutableArray<FakeHubClientToGenerate>.Empty;
        }

        var results = ImmutableArray.CreateBuilder<FakeHubClientToGenerate>();

        foreach (var attribute in context.Attributes)
        {
            if (attribute.AttributeClass is null ||
                !attribute.AttributeClass.Equals(markerAttribute, SymbolEqualityComparer.Default))
            {
                continue;
            }

            var hubClientAttributeArgument = attribute.ConstructorArguments.FirstOrDefault();

            if (hubClientAttributeArgument.Value is not INamedTypeSymbol hubClientTypeSymbol)
            {
                continue;
            }

            var hubClientNamespace = hubClientTypeSymbol.ContainingNamespace.ToString();
            var hubClientName = hubClientTypeSymbol.Name;

            var extractor = new HubClientMethodExtractor(context.SemanticModel, hubClientTypeSymbol);
            var extractedData = extractor.Extract();

            results.Add(new FakeHubClientToGenerate(hubClientNamespace, hubClientName, extractedData.ClientToServerMethods,
                extractedData.ServerToClientMethods));
        }

        return results.ToImmutable();
    }
}