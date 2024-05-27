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
    private const string ServerToClientAttributeFullQualifiedName = "SignalRGen.Generator.ServerToClientMethodAttribute";
    private const string ClientToServerAttributeFullQualifiedName = "SignalRGen.Generator.ClientToServerMethodAttribute";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        Debugger.Launch();
        context.RegisterPostInitializationOutput(ctx =>
            ctx.AddSource("HubClientAttribute.g.cs", HubClientAttributeSource.GetSource()));
        context.RegisterPostInitializationOutput(ctx =>
            ctx.AddSource("ServerToClientMethodAttribute.g.cs", ServerToClientMethodAttributeSource.GetSource()));
        context.RegisterPostInitializationOutput(ctx =>
            ctx.AddSource("ClientToServerMethodAttribute.g.cs", ClientToServerMethodAttributeSource.GetSource()));
        context.RegisterPostInitializationOutput(ctx =>
            ctx.AddSource("HubClientBase.g.cs", HubClientBaseSource.GetSource()));
        context.RegisterPostInitializationOutput(ctx =>
            ctx.AddSource("HubClientOptions.g.cs", HubClientOptionsSource.GetSource()));
        context.RegisterPostInitializationOutput(ctx => 
            ctx.AddSource("IHubClient.g.cs", IHubClientSource.GetSource()));
        context.RegisterPostInitializationOutput(ctx =>
            ctx.AddSource("SignalRHubServiceCollection.g.cs", FmSignalRHubServiceCollectionSource.GetSource()));
        context.RegisterPostInitializationOutput(ctx =>
            ctx.AddSource("SignalROptions.g.cs", SignalROptionsSource.GetSource()));

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

    private static HubClientToGenerate? GetSemanticTargetForGeneration(GeneratorAttributeSyntaxContext context,
        CancellationToken cancellationToken)
    {
        var node = (context.TargetNode as InterfaceDeclarationSyntax)!;

        var markerAttribute =
            context.SemanticModel.Compilation.GetTypeByMetadataName(MarkerAttributeFullQualifiedName);

        if (markerAttribute is null)
        {
            return null;
        }

        var serverToClientAttribute =
            context.SemanticModel.Compilation.GetTypeByMetadataName(ServerToClientAttributeFullQualifiedName);

        var clientToServerAttribute =
            context.SemanticModel.Compilation.GetTypeByMetadataName(ClientToServerAttributeFullQualifiedName);

        if (serverToClientAttribute is null || clientToServerAttribute is null)
        {
            return null;
        }

        var hubClientAttribute = context.Attributes.FirstOrDefault(x =>
            x.AttributeClass is not null
            && x.AttributeClass.Equals(markerAttribute, SymbolEqualityComparer.Default));

        var usings = GetInterfacesUsings(node);
        // var methods = GetInterfaceMethods(node);

        var serverToClientMethods = new List<CacheableMethodDeclaration>();
        var clientToServerMethods = new List<CacheableMethodDeclaration>();
        foreach (var method in node.Members.OfType<MethodDeclarationSyntax>())
        {
            var cacheableMethodDeclaration = new CacheableMethodDeclaration(method.Identifier.Text,
                method.ParameterList.Parameters.Select(p => new Parameter(p.Type.ToString(), p.Identifier.Text))
                    .ToImmutableArray().AsEquatableArray());
            if (method.AttributeLists.Count == 0)
            {
                serverToClientMethods.Add(cacheableMethodDeclaration);
            }

            foreach (var attributeList in method.AttributeLists)
            {
                foreach (var attribute in attributeList.Attributes)
                {
                    var attributeSymbol = context.SemanticModel.GetSymbolInfo(attribute).Symbol;

                    if (attributeSymbol is not null &&
                        attributeSymbol.ContainingType.Equals(clientToServerAttribute, SymbolEqualityComparer.Default))
                    {
                        clientToServerMethods.Add(cacheableMethodDeclaration);
                        break;
                    }
                    
                    // We are not checking here if the `ServerToClientAttribute` is applied, as this is the default case.
                    // Maybe this should be re-evaluated at a later date
                    
                    serverToClientMethods.Add(cacheableMethodDeclaration);
                }
            }
        }

        return hubClientAttribute is null
            ? null
            : new HubClientToGenerate(InterfaceName: node.Identifier.Text,
                HubName: GetHubNameOrDefaultConvention(hubClientAttribute, node), HubUri: GetHubUri(hubClientAttribute),
                InterfaceNamespace: GetInterfaceNamespace(node), Usings: usings, ServerToClientMethods: serverToClientMethods.ToImmutableArray().AsEquatableArray());
    }

    private static string GetHubUri(AttributeData hubClientAttribute)
    {
        var hubUri = hubClientAttribute.NamedArguments
            .First(a => a.Key == "HubUri").Value.Value!
            .ToString();

        return hubUri;
    }

    private static string GetInterfaceNamespace(InterfaceDeclarationSyntax interfaceDeclarationSyntax)
    {
        return interfaceDeclarationSyntax.FirstAncestorOrSelf<BaseNamespaceDeclarationSyntax>()?.Name.ToString() ??
               "Borked";
    }

    private static EquatableArray<CacheableMethodDeclaration> GetInterfaceMethods(TypeDeclarationSyntax node)
    {
        // return node.Members.OfType<MethodDeclarationSyntax>();
        var methods = node.Members.OfType<MethodDeclarationSyntax>();
        return methods.Select(m => new CacheableMethodDeclaration(m.Identifier.Text,
            m.ParameterList.Parameters.Select(p => new Parameter(p.Type.ToString(), p.Identifier.Text))
                .ToImmutableArray().AsEquatableArray())).ToImmutableArray().AsEquatableArray();
    }

    private static EquatableArray<CacheableUsingDeclaration> GetInterfacesUsings(SyntaxNode syntaxNode)
    {
        // return syntaxNode.Parent?.Parent?.ChildNodes().OfType<UsingDirectiveSyntax>()
        //        ?? Enumerable.Empty<UsingDirectiveSyntax>();
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


        var rValue = hubName?.ToString() ?? syntaxNode.Identifier.Text.Substring(1);
        return rValue;
    }
}