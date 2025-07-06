using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SignalRGen.Analyzers.Extensions;
using SignalRGen.Shared;

namespace SignalRGen.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class ClientToServerAnalyzer : DiagnosticAnalyzer
{
    public static readonly DiagnosticDescriptor ClientToServerReturnTypeRule = new(
        id: DiagnosticIds.SRG0003OnlyTaskOrTaskOfTMethodsAllowedInClientToServerHubContract,
        title: "Client-to-server methods must return Task or Task<T>",
        messageFormat: "Method {0} in client-to-server interface {1} must return Task or Task<T>",
        category: "SignalRGen",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description:
        "Client-to-server interfaces should only contain methods that return Task or Task<T> since SignalRGen only supports async clients."
    );

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [ClientToServerReturnTypeRule];
    
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSymbolAction(AnalyzeInterfaceDeclaration, SymbolKind.NamedType);
    }

    private static void AnalyzeInterfaceDeclaration(SymbolAnalysisContext context)
    {
        if (context.Symbol is not INamedTypeSymbol interfaceSymbol) return;

        if (interfaceSymbol.TypeKind != TypeKind.Interface) return;

        if (!IsClientToServerInterface(interfaceSymbol, context.Compilation, context.CancellationToken)) return;
        
        var methods = interfaceSymbol.GetMembers();
        foreach (var method in methods)
        {
            if (method is not IMethodSymbol methodSymbol) continue;

            if (!IsValidReturnType(methodSymbol.ReturnType))
            {
                var locationForDiagnostic = methodSymbol.Locations.First();
                var declSyntax = methodSymbol.DeclaringSyntaxReferences.FirstOrDefault()
                    ?.GetSyntax(context.CancellationToken) as MethodDeclarationSyntax;
                
                locationForDiagnostic = declSyntax?.ReturnType.GetLocation() ?? locationForDiagnostic;

                var diagnostic = Diagnostic.Create(
                    ClientToServerReturnTypeRule,
                    locationForDiagnostic,
                    methodSymbol.Name,
                    interfaceSymbol.Name,
                    methodSymbol.ReturnType.ToDisplayString()
                );
                
                context.ReportDiagnostic(diagnostic);
            }
        }
    }

    private static bool IsClientToServerInterface(INamedTypeSymbol interfaceSymbol, Compilation compilation, CancellationToken ct = default)
    {
        var allTypes = compilation.GetAllNamedTypes(ct);

        foreach (var type in allTypes)
        {
            // Check if this type implements IBidirectionalHub<TServer, TClient> where TServer is our interface
            foreach (var implementedInterface in type.AllInterfaces)
            {
                if (implementedInterface.Name == "IBidirectionalHub" &&
                    implementedInterface.TypeArguments.Length == 2)
                {
                    var clientType = implementedInterface.TypeArguments[1];
                    if (SymbolEqualityComparer.Default.Equals(clientType, interfaceSymbol))
                    {
                        return true;
                    }
                }

                // Check if this type implements IClientToServerHub<TClient> where TClient is our interface
                if (implementedInterface.Name == "IClientToServerHub" &&
                    implementedInterface.TypeArguments.Length == 1)
                {
                    var clientType = implementedInterface.TypeArguments[0];
                    if (SymbolEqualityComparer.Default.Equals(clientType, interfaceSymbol))
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }
    
    private static bool IsValidReturnType(ITypeSymbol returnType)
    {
        return returnType is INamedTypeSymbol namedType &&
               namedType.Name == "Task" &&
               namedType.ContainingNamespace.ToDisplayString() == "System.Threading.Tasks";
    }
}