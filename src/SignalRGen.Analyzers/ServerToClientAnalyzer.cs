using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

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

            if (!IsGenericTask(methodSymbol.ReturnType)) continue;
            
            var locationForDiagnostic = methodSymbol.Locations.First();
            var declSyntax = methodSymbol.DeclaringSyntaxReferences.FirstOrDefault()
                ?.GetSyntax(context.CancellationToken) as MethodDeclarationSyntax;

            var genericReturn = declSyntax?.ReturnType as GenericNameSyntax;
            var typeArgList   = genericReturn?.TypeArgumentList;

            if (typeArgList != null)
                locationForDiagnostic = typeArgList.GetLocation(); 
            
            var diagnostic = Diagnostic.Create(
                ServerToClientReturnTypeRule,
                locationForDiagnostic,
                methodSymbol.Name,
                interfaceSymbol.Name,
                methodSymbol.ReturnType.ToDisplayString());

            context.ReportDiagnostic(diagnostic);
        }
    }

    private static bool IsServerToClientInterface(INamedTypeSymbol interfaceSymbol, Compilation compilation, CancellationToken ct = default)
    {
        // Find all types that reference this interface as a server type parameter
        var allTypes = GetAllNamedTypes(compilation, ct);

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

    private static IEnumerable<INamedTypeSymbol> GetAllNamedTypes(Compilation compilation, CancellationToken ct)
    {
        var stack = new Stack<INamespaceOrTypeSymbol>();
        stack.Push(compilation.GlobalNamespace);

        while (stack.Count > 0)
        {
            ct.ThrowIfCancellationRequested();

            var current = stack.Pop();

            switch (current)
            {
                case INamespaceSymbol ns:
                    foreach (var member in ns.GetMembers())
                        stack.Push(member);
                    break;

                case INamedTypeSymbol typeSymbol:
                    yield return typeSymbol;

                    foreach (var nested in typeSymbol.GetTypeMembers())
                        stack.Push(nested);
                    break;
            }
        }
    }

    private static bool IsGenericTask(ITypeSymbol returnType)
    {
        return returnType is INamedTypeSymbol namedType &&
               namedType.Name == "Task" &&
               namedType.ContainingNamespace.ToDisplayString() == "System.Threading.Tasks" &&
               namedType.TypeArguments.Length > 0;
    }
}