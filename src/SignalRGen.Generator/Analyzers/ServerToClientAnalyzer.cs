using System.Collections.Immutable;
using System.Composition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace SignalRGen.Generator.Analyzers;

#region Analyzer

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class ServerToClientAnalyzer : DiagnosticAnalyzer
{
    public static readonly DiagnosticDescriptor ServerToClientReturnTypeRule = new DiagnosticDescriptor(
        id: "SRGN0002",
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

        if (ModelExtensions.GetDeclaredSymbol(semanticModel, interfaceDeclaration) is not INamedTypeSymbol
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

#endregion

#region CodeFix


[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ServerToClientReturnTypeCodeFixProvider)), Shared]
public class ServerToClientReturnTypeCodeFixProvider : CodeFixProvider
{
    public sealed override ImmutableArray<string> FixableDiagnosticIds =>
        [ServerToClientAnalyzer.ServerToClientReturnTypeRule.Id];

    public sealed override FixAllProvider GetFixAllProvider() => new ServerToClientFixAllProvider();

    public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        if (root is null) return;

        // Group diagnostics by location to avoid duplicate fixes for the same method, but even this doesn't seem to work :(
        var diagnosticsGrouped = context.Diagnostics
            .Where(d => FixableDiagnosticIds.Contains(d.Id))
            .GroupBy(d => d.Location.SourceSpan)
            .ToList();
        
        foreach (var diagnosticGroup in diagnosticsGrouped)
        {
            var diagnostic = diagnosticGroup.First();
            
            var diagnosticSpan = diagnostic.Location.SourceSpan;
            var returnTypeNode = root.FindNode(diagnosticSpan);
            
            var methodDeclaration = returnTypeNode.Ancestors().OfType<MethodDeclarationSyntax>().FirstOrDefault();
            if (methodDeclaration is null) continue;
            
            var interfaceDeclaration = methodDeclaration.Ancestors().OfType<InterfaceDeclarationSyntax>().FirstOrDefault();
            if (interfaceDeclaration is null) continue;

            var semanticModel = await context.Document.GetSemanticModelAsync(context.CancellationToken).ConfigureAwait(false);
            if (semanticModel is null) continue;

            var interfaceSymbol = semanticModel.GetDeclaredSymbol(interfaceDeclaration);
            if (interfaceSymbol is null) continue;
            
            var baseKey = $"{interfaceSymbol.ToDisplayString()}_{methodDeclaration.Identifier.ValueText}_{methodDeclaration.Span.Start}";

            var action = CodeAction.Create(
                title: "Change return type to Task",
                createChangedDocument: c => ConvertTaskTToTask(context.Document, methodDeclaration, c),
                equivalenceKey: $"{baseKey}_ConvertToTask");

            context.RegisterCodeFix(action, diagnostic);
        }
    }

    private static async Task<Document> ConvertTaskTToTask(
        Document document,
        MethodDeclarationSyntax methodDeclaration,
        CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        if (root is null) return document;
        
        var newReturnType = SyntaxFactory.IdentifierName("Task")
            .WithTriviaFrom(methodDeclaration.ReturnType);
        
        var newMethodDeclaration = methodDeclaration.WithReturnType(newReturnType);
        
        var newRoot = root.ReplaceNode(methodDeclaration, newMethodDeclaration);

        return document.WithSyntaxRoot(newRoot);
    }
}

public class ServerToClientFixAllProvider : FixAllProvider
{
    public override IEnumerable<FixAllScope> GetSupportedFixAllScopes()
    {
        return new[]
        {
            FixAllScope.Document,
            FixAllScope.Project,
            FixAllScope.Solution
        };
    }

    public override Task<CodeAction?> GetFixAsync(FixAllContext fixAllContext)
    {
        return Task.FromResult<CodeAction?>(CodeAction.Create(
            title: "Change all Task<T> to Task",
            createChangedSolution: ct => FixAllAsync(fixAllContext, ct),
            equivalenceKey: "FixAllServerToClientReturnTypes"));
    }

    private static async Task<Solution> FixAllAsync(FixAllContext fixAllContext, CancellationToken cancellationToken)
    {
        var solution = fixAllContext.Solution;
        
        var documentsAndDiagnostics = await GetDocumentsAndDiagnosticsAsync(fixAllContext, cancellationToken);
        
        var documentGroups = documentsAndDiagnostics.GroupBy(x => x.Document);
        
        foreach (var documentGroup in documentGroups)
        {
            var document = documentGroup.Key;
            var diagnostics = documentGroup.SelectMany(x => x.Diagnostics).ToList();
            
            if (diagnostics.Count == 0) continue;
            
            var currentDocument = solution.GetDocument(document.Id);
            if (currentDocument == null) continue;
            
            var updatedDocument = await FixAllInDocumentAsync(currentDocument, diagnostics, cancellationToken);
            solution = updatedDocument.Project.Solution;
        }
        
        return solution;
    }

    private static async Task<IEnumerable<(Document Document, IEnumerable<Diagnostic> Diagnostics)>> GetDocumentsAndDiagnosticsAsync(
        FixAllContext fixAllContext, 
        CancellationToken cancellationToken)
    {
        if (fixAllContext.Document is null)
        {
            return [];
        }
        var result = new List<(Document, IEnumerable<Diagnostic>)>();
        
        switch (fixAllContext.Scope)
        {
            case FixAllScope.Document:
                var documentDiagnostics = await fixAllContext.GetDocumentDiagnosticsAsync(fixAllContext.Document);
                result.Add((fixAllContext.Document, documentDiagnostics));
                break;
                
            case FixAllScope.Project:
                foreach (var document in fixAllContext.Project.Documents)
                {
                    var projectDiagnostics = await fixAllContext.GetDocumentDiagnosticsAsync(document);
                    if (projectDiagnostics.Any())
                    {
                        result.Add((document, projectDiagnostics));
                    }
                }
                break;
                
            case FixAllScope.Solution:
                foreach (var project in fixAllContext.Solution.Projects)
                {
                    foreach (var document in project.Documents)
                    {
                        var solutionDiagnostics = await fixAllContext.GetDocumentDiagnosticsAsync(document);
                        if (solutionDiagnostics.Any())
                        {
                            result.Add((document, solutionDiagnostics));
                        }
                    }
                }
                break;
        }
        
        return result;
    }

    private static async Task<Document> FixAllInDocumentAsync(
        Document document,
        IEnumerable<Diagnostic> diagnostics,
        CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        if (root == null) return document;
        
        var methodsToFix = new List<(MethodDeclarationSyntax Original, MethodDeclarationSyntax Fixed)>();

        foreach (var diagnostic in diagnostics)
        {
            var diagnosticSpan = diagnostic.Location.SourceSpan;
            var returnTypeNode = root.FindNode(diagnosticSpan);
            
            var methodDeclaration = returnTypeNode.Ancestors().OfType<MethodDeclarationSyntax>().FirstOrDefault();
            if (methodDeclaration == null) continue;
            
            var newReturnType = SyntaxFactory.IdentifierName("Task")
                .WithTriviaFrom(methodDeclaration.ReturnType);
            
            var newMethodDeclaration = methodDeclaration.WithReturnType(newReturnType);
            
            methodsToFix.Add((methodDeclaration, newMethodDeclaration));
        }
        
        if (methodsToFix.Count > 0)
        {
            var nodesToReplace = methodsToFix.ToDictionary(x => (SyntaxNode)x.Original, x => (SyntaxNode)x.Fixed);
            var newRoot = root.ReplaceNodes(nodesToReplace.Keys, (original, _) => nodesToReplace[original]);
            return document.WithSyntaxRoot(newRoot);
        }

        return document;
    }
}

#endregion