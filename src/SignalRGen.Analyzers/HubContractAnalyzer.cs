using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using SignalRGen.Shared;

namespace SignalRGen.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class HubContractAnalyzer : DiagnosticAnalyzer
{
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

        var methods = interfaceSymbol.GetMembers();
        foreach (var method in methods)
        {
            var diagnostic = Diagnostic.Create(
                MethodInHubInterfaceRule,
                method.Locations.FirstOrDefault(),
                method.Name,
                interfaceSymbol.Name);

            context.ReportDiagnostic(diagnostic);
        }
    }

    private static bool InheritsFromHubInterface(INamedTypeSymbol interfaceSymbol)
    {
        foreach (var baseInterface in interfaceSymbol.AllInterfaces)
        {
            var baseInterfaceName = baseInterface.Name;

            if (baseInterfaceName == "IBidirectionalHub" ||
                baseInterfaceName == "IServerToClientHub" ||
                baseInterfaceName == "IClientToServerHub")
            {
                return true;
            }
        }

        return false;
    }
}
