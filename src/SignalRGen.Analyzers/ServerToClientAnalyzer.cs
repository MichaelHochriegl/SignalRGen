using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace SignalRGen.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class ServerToClientAnalyzer : DiagnosticAnalyzer
{
    public static readonly DiagnosticDescriptor ServerToClientReturnTypeRule = new DiagnosticDescriptor(
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
        context.RegisterSyntaxNodeAction(AnalyzeInterfaceDeclaration, SyntaxKind.InterfaceDeclaration);
    }

    private static void AnalyzeInterfaceDeclaration(SyntaxNodeAnalysisContext context)
    {
        var interfaceDeclaration = (InterfaceDeclarationSyntax)context.Node;
        var semanticModel = context.SemanticModel;

        if (semanticModel.GetDeclaredSymbol(interfaceDeclaration) is not INamedTypeSymbol
            interfaceSymbol)
            return;

        // Check if this interface is used as a server-to-client interface
        if (!IsServerToClientInterface(interfaceSymbol, context.Compilation))
            return;

        // Analyze all methods in this interface
        var methods = interfaceDeclaration.Members.OfType<MethodDeclarationSyntax>();
        foreach (var method in methods)
        {
            var methodSymbol = semanticModel.GetDeclaredSymbol(method);
            if (methodSymbol == null) continue;

            // Check if the return type is Task<T> instead of Task
            if (IsGenericTask(methodSymbol.ReturnType))
            {
                var diagnostic = Diagnostic.Create(
                    ServerToClientReturnTypeRule,
                    method.ReturnType.GetLocation(),
                    method.Identifier.ValueText,
                    interfaceSymbol.Name,
                    methodSymbol.ReturnType.ToDisplayString());

                context.ReportDiagnostic(diagnostic);
            }
        }
    }

    private static bool IsServerToClientInterface(INamedTypeSymbol interfaceSymbol, Compilation compilation)
    {
        // Find all types that reference this interface as a server type parameter
        var allTypes = GetAllNamedTypes(compilation);

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

    private static IEnumerable<INamedTypeSymbol> GetAllNamedTypes(Compilation compilation)
    {
        var allTypes = new List<INamedTypeSymbol>();

        foreach (var syntaxTree in compilation.SyntaxTrees)
        {
            var semanticModel = compilation.GetSemanticModel(syntaxTree);
            var root = syntaxTree.GetRoot();

            var typeDeclarations = root.DescendantNodes().OfType<TypeDeclarationSyntax>();
            foreach (var typeDecl in typeDeclarations)
            {
                if (semanticModel.GetDeclaredSymbol(typeDecl) is INamedTypeSymbol typeSymbol)
                {
                    allTypes.Add(typeSymbol);
                }
            }
        }

        return allTypes;
    }

    private static bool IsGenericTask(ITypeSymbol returnType)
    {
        // Check if it's Task<T> (generic Task with type arguments)
        return returnType is INamedTypeSymbol namedType &&
               namedType.Name == "Task" &&
               namedType.ContainingNamespace.ToDisplayString() == "System.Threading.Tasks" &&
               namedType.TypeArguments.Length > 0;
    }
}