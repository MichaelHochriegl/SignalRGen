using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SignalRGen.Analyzers.Extensions;
using SignalRGen.Shared;

namespace SignalRGen.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class ServerToClientAnalyzer : DiagnosticAnalyzer
{
    public static readonly DiagnosticDescriptor ServerToClientReturnTypeRule = new(
        id: DiagnosticIds.SRG0002OnlyTaskMethodsAllowedInServerToClientHubContract,
        title: "Server-to-client methods must return Task, not Task<T>",
        messageFormat:
        "Method '{0}' in server-to-client interface '{1}' must return Task, not '{2}'. Server-to-client methods cannot have return values.",
        category: "SignalRGen",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description:
        "Server-to-client interfaces should only contain methods that return Task (not Task<T>) since SignalR server-to-client calls are fire-and-forget operations that don't return values to the server.");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        [ServerToClientReturnTypeRule];

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

        if (!IsServerToClientInterface(interfaceSymbol, context.Compilation, context.CancellationToken))
            return;
        
        var methods = interfaceSymbol.GetMembers();
        foreach (var method in methods)
        {
            if (method is not IMethodSymbol methodSymbol) continue;

            if (!IsValidReturnType(methodSymbol.ReturnType))
            {
                var locationForDiagnostic = methodSymbol.Locations.First();
                var declSyntax = methodSymbol.DeclaringSyntaxReferences.FirstOrDefault()
                    ?.GetSyntax(context.CancellationToken) as MethodDeclarationSyntax;

                // For Task<T>, highlight the type argument list
                if (IsTaskWithGenericArguments(methodSymbol.ReturnType))
                {
                    var genericReturn = declSyntax?.ReturnType as GenericNameSyntax;
                    var typeArgList = genericReturn?.TypeArgumentList;
                    if (typeArgList != null)
                        locationForDiagnostic = typeArgList.GetLocation();
                }
                // For other return types, highlight the entire return type
                else if (declSyntax?.ReturnType != null)
                {
                    locationForDiagnostic = declSyntax.ReturnType.GetLocation();
                }

                var diagnostic = Diagnostic.Create(
                    ServerToClientReturnTypeRule,
                    locationForDiagnostic,
                    methodSymbol.Name,
                    interfaceSymbol.Name,
                    methodSymbol.ReturnType.ToDisplayString());

                context.ReportDiagnostic(diagnostic);
            }

        }
    }

    private static bool IsServerToClientInterface(INamedTypeSymbol interfaceSymbol, Compilation compilation, CancellationToken ct = default)
    {
        // Find all types that reference this interface as a server type parameter
        var allTypes = compilation.GetAllNamedTypes(ct);

        foreach (var type in allTypes)
        {
            // Check if this type implements IBidirectionalHub<TServer, TClient> where TServer is our interface
            foreach (var implementedInterface in type.AllInterfaces)
            {
                if (implementedInterface.Name == "IBidirectionalHub" &&
                    implementedInterface.TypeArguments.Length == 2)
                {
                    var serverType = implementedInterface.TypeArguments[0];
                    if (SymbolEqualityComparer.Default.Equals(serverType, interfaceSymbol))
                    {
                        return true;
                    }
                }

                // Check if this type implements IServerToClientHub<TServer> where TServer is our interface
                if (implementedInterface.Name == "IServerToClientHub" &&
                    implementedInterface.TypeArguments.Length == 1)
                {
                    var serverType = implementedInterface.TypeArguments[0];
                    if (SymbolEqualityComparer.Default.Equals(serverType, interfaceSymbol))
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
               namedType.ContainingNamespace.ToDisplayString() == "System.Threading.Tasks" &&
               namedType.TypeArguments.Length == 0; // Must be non-generic Task
    }

    private static bool IsTaskWithGenericArguments(ITypeSymbol returnType)
    {
        return returnType is INamedTypeSymbol namedType &&
               namedType.Name == "Task" &&
               namedType.ContainingNamespace.ToDisplayString() == "System.Threading.Tasks" &&
               namedType.TypeArguments.Length > 0;
    }

}