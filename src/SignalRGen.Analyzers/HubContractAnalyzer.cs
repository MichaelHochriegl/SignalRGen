using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SignalRGen.Shared;

namespace SignalRGen.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class HubContractAnalyzer : DiagnosticAnalyzer
{
    private static readonly ImmutableHashSet<string> HubInterfaceNames = ImmutableHashSet.Create(
        "IBidirectionalHub", "IServerToClientHub", "IClientToServerHub");

    public static readonly DiagnosticDescriptor MethodInHubInterfaceRule = new(
        id: DiagnosticIds.SRG0001NoMethodsInHubContractAllowed,
        title: "Methods should not be declared in hub interfaces",
        messageFormat:
        "Method '{0}' should not be declared in interface '{1}' that inherits from a hub interface. Move methods to the appropriate client or server interface.",
        category: "SignalRGen",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description:
        "Hub interfaces that inherit from IBidirectionalHub, IServerToClientHub, or IClientToServerHub should not contain method declarations. Methods should be placed in the appropriate client or server interfaces.");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        [MethodInHubInterfaceRule];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSymbolAction(AnalyzeInterfaceDeclaration, SymbolKind.NamedType);
    }

    private static void AnalyzeInterfaceDeclaration(SymbolAnalysisContext context)
    {
        if (context.Symbol is not INamedTypeSymbol interfaceSymbol) return;
        
        if (interfaceSymbol.TypeKind != TypeKind.Interface)
            return;

        if (!InheritsFromHubInterface(interfaceSymbol))
            return;
        
        var methods = interfaceSymbol.GetMembers().OfType<IMethodSymbol>();
        
        foreach (var method in methods)
        {
            var location = GetMethodDeclarationLocation(method);
            if (location is null) continue;
            
            var diagnostic = Diagnostic.Create(
                MethodInHubInterfaceRule,
                location,
                method.Name,
                interfaceSymbol.Name);

            context.ReportDiagnostic(diagnostic);
        }
    }

    private static Location? GetMethodDeclarationLocation(IMethodSymbol method)
    {
        var location = method.Locations.FirstOrDefault();
        if (location?.SourceTree is null)
            return location;

        var root = location.SourceTree.GetRoot();
        var methodNode = root.FindNode(location.SourceSpan);
        
        var methodDeclaration = methodNode.AncestorsAndSelf()
            .OfType<MethodDeclarationSyntax>()
            .FirstOrDefault();
        
        return methodDeclaration?.GetLocation() ?? location;
    }

    private static bool InheritsFromHubInterface(INamedTypeSymbol interfaceSymbol)
    {
        return interfaceSymbol.AllInterfaces.Any(baseInterface => 
            HubInterfaceNames.Contains(baseInterface.Name));
    }
}