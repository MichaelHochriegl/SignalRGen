using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace SignalRGen.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class HubContractAnalyzer : DiagnosticAnalyzer
{
    public static readonly DiagnosticDescriptor MethodInHubInterfaceRule = new DiagnosticDescriptor(
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
        context.RegisterSyntaxNodeAction(AnalyzeInterfaceDeclaration, SyntaxKind.InterfaceDeclaration);
    }

    private static void AnalyzeInterfaceDeclaration(SyntaxNodeAnalysisContext context)
    {
        var interfaceDeclaration = (InterfaceDeclarationSyntax)context.Node;
        var semanticModel = context.SemanticModel;

        if (semanticModel.GetDeclaredSymbol(interfaceDeclaration) is not INamedTypeSymbol
            interfaceSymbol)
            return;

        if (!InheritsFromHubInterface(interfaceSymbol))
            return;

        var methods = interfaceDeclaration.Members.OfType<MethodDeclarationSyntax>();
        foreach (var method in methods)
        {
            var diagnostic = Diagnostic.Create(
                MethodInHubInterfaceRule,
                method.GetLocation(),
                method.Identifier.ValueText,
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
