using System.Collections.Immutable;
using System.Composition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SignalRGen.Shared;

namespace SignalRGen.CodeFixes;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(HubInterfaceMethodCodeFixProvider)), Shared]
public class HubInterfaceMethodCodeFixProvider : CodeFixProvider
{
    private static readonly ImmutableHashSet<string> HubInterfaceNames = ImmutableHashSet.Create(
        "IBidirectionalHub", "IServerToClientHub", "IClientToServerHub");

    public sealed override ImmutableArray<string> FixableDiagnosticIds =>
        [DiagnosticIds.SRG0001NoMethodsInHubContractAllowed];

    public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var diagnostic = context.Diagnostics.FirstOrDefault(d => d.Id == DiagnosticIds.SRG0001NoMethodsInHubContractAllowed);
        if (diagnostic is null) return;

        var analysisResult = await AnalyzeCodeFixContextAsync(context, diagnostic);
        if (analysisResult is null) return;

        var baseKey = CreateBaseKey(analysisResult.InterfaceSymbol, analysisResult.MethodDeclaration);
        
        RegisterCodeFixActions(context, diagnostic, analysisResult, baseKey);
    }

    private static async Task<CodeFixAnalysisResult?> AnalyzeCodeFixContextAsync(
        CodeFixContext context, 
        Diagnostic diagnostic)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        if (root is null) return null;

        var methodDeclaration = FindMethodDeclaration(root, diagnostic);
        if (methodDeclaration is null) return null;

        var interfaceDeclaration = methodDeclaration.Ancestors().OfType<InterfaceDeclarationSyntax>().FirstOrDefault();
        if (interfaceDeclaration is null) return null;

        var semanticModel = await context.Document.GetSemanticModelAsync(context.CancellationToken).ConfigureAwait(false);
        if (semanticModel is null) return null;

        var interfaceSymbol = semanticModel.GetDeclaredSymbol(interfaceDeclaration);
        if (interfaceSymbol is null) return null;

        var hubInterface = FindHubInterface(interfaceSymbol);
        if (hubInterface is null) return null;

        var (serverType, clientType) = ExtractGenericTypes(hubInterface);

        return new CodeFixAnalysisResult(
            methodDeclaration,
            interfaceDeclaration,
            interfaceSymbol,
            serverType,
            clientType);
    }

    private static MethodDeclarationSyntax? FindMethodDeclaration(SyntaxNode root, Diagnostic diagnostic)
    {
        var diagnosticSpan = diagnostic.Location.SourceSpan;
        return root.FindToken(diagnosticSpan.Start).Parent?
            .AncestorsAndSelf()
            .OfType<MethodDeclarationSyntax>()
            .FirstOrDefault();
    }

    private static void RegisterCodeFixActions(
        CodeFixContext context,
        Diagnostic diagnostic,
        CodeFixAnalysisResult analysisResult,
        string baseKey)
    {
        if (analysisResult.ServerType is not null)
        {
            var moveToServerAction = CodeAction.Create(
                title: $"Move to server interface ({analysisResult.ServerType.Name})",
                createChangedSolution: c => MoveMethodToInterface(
                    context.Document,
                    analysisResult.MethodDeclaration,
                    analysisResult.InterfaceDeclaration,
                    analysisResult.ServerType,
                    c),
                equivalenceKey: $"{nameof(HubInterfaceMethodCodeFixProvider)}_{baseKey}_Server_{analysisResult.ServerType.ToDisplayString()}");

            context.RegisterCodeFix(moveToServerAction, diagnostic);
        }

        if (analysisResult.ClientType is not null)
        {
            var moveToClientAction = CodeAction.Create(
                title: $"Move to client interface ({analysisResult.ClientType.Name})",
                createChangedSolution: c => MoveMethodToInterface(
                    context.Document,
                    analysisResult.MethodDeclaration,
                    analysisResult.InterfaceDeclaration,
                    analysisResult.ClientType,
                    c),
                equivalenceKey: $"{nameof(HubInterfaceMethodCodeFixProvider)}_{baseKey}_Client_{analysisResult.ClientType.ToDisplayString()}");

            context.RegisterCodeFix(moveToClientAction, diagnostic);
        }
    }

    private static string CreateBaseKey(INamedTypeSymbol interfaceSymbol, MethodDeclarationSyntax methodDeclaration)
    {
        return $"{interfaceSymbol.ToDisplayString()}_{methodDeclaration.Identifier.ValueText}_{methodDeclaration.Span.Start}";
    }

    private static INamedTypeSymbol? FindHubInterface(INamedTypeSymbol interfaceSymbol)
    {
        return interfaceSymbol.AllInterfaces.FirstOrDefault(i => HubInterfaceNames.Contains(i.Name));
    }

    private static (INamedTypeSymbol? serverType, INamedTypeSymbol? clientType) ExtractGenericTypes(
        INamedTypeSymbol hubInterface)
    {
        return hubInterface.Name switch
        {
            "IBidirectionalHub" when hubInterface.TypeArguments.Length == 2 =>
                (hubInterface.TypeArguments[0] as INamedTypeSymbol, hubInterface.TypeArguments[1] as INamedTypeSymbol),
            "IServerToClientHub" when hubInterface.TypeArguments.Length == 1 =>
                (hubInterface.TypeArguments[0] as INamedTypeSymbol, null),
            "IClientToServerHub" when hubInterface.TypeArguments.Length == 1 =>
                (null, hubInterface.TypeArguments[0] as INamedTypeSymbol),
            _ => (null, null)
        };
    }

    private static async Task<Solution> MoveMethodToInterface(
        Document document,
        MethodDeclarationSyntax methodDeclaration,
        InterfaceDeclarationSyntax sourceInterface,
        INamedTypeSymbol targetInterface,
        CancellationToken cancellationToken)
    {
        var solution = document.Project.Solution;
        var targetDocument = await FindInterfaceDocumentAsync(solution, targetInterface, cancellationToken);
        
        if (targetDocument is null) return solution;

        return document.Id == targetDocument.Id
            ? await MoveBothInterfacesInSameDocument(document, methodDeclaration, sourceInterface, targetInterface, cancellationToken)
            : await MoveBetweenDifferentDocuments(document, targetDocument, methodDeclaration, sourceInterface, targetInterface, cancellationToken);
    }

    private static async Task<Solution> MoveBothInterfacesInSameDocument(
        Document document,
        MethodDeclarationSyntax methodDeclaration,
        InterfaceDeclarationSyntax sourceInterface,
        INamedTypeSymbol targetInterface,
        CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        if (root is null) return document.Project.Solution;

        var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
        if (semanticModel is null) return document.Project.Solution;

        var targetInterfaceDeclaration = FindTargetInterfaceDeclaration(root, semanticModel, targetInterface);
        if (targetInterfaceDeclaration is null) return document.Project.Solution;

        var cleanMethodDeclaration = CreateCleanMethodDeclaration(methodDeclaration);
        var newSourceInterface = sourceInterface.RemoveNode(methodDeclaration, SyntaxRemoveOptions.KeepNoTrivia);
        if (newSourceInterface is null) return document.Project.Solution;

        var newTargetInterface = targetInterfaceDeclaration.AddMembers(cleanMethodDeclaration);

        var newRoot = root.ReplaceNodes(
            [sourceInterface, targetInterfaceDeclaration],
            (original, _) => original == sourceInterface ? newSourceInterface : newTargetInterface);

        return document.Project.Solution.WithDocumentSyntaxRoot(document.Id, newRoot);
    }

    private static async Task<Solution> MoveBetweenDifferentDocuments(
        Document sourceDocument,
        Document targetDocument,
        MethodDeclarationSyntax methodDeclaration,
        InterfaceDeclarationSyntax sourceInterface,
        INamedTypeSymbol targetInterface,
        CancellationToken cancellationToken)
    {
        var solution = sourceDocument.Project.Solution;
        
        solution = await RemoveMethodFromSourceDocument(solution, sourceDocument, methodDeclaration, sourceInterface, cancellationToken);
        
        solution = await AddMethodToTargetDocument(solution, targetDocument, methodDeclaration, targetInterface, cancellationToken);

        return solution;
    }

    private static async Task<Solution> RemoveMethodFromSourceDocument(
        Solution solution,
        Document sourceDocument,
        MethodDeclarationSyntax methodDeclaration,
        InterfaceDeclarationSyntax sourceInterface,
        CancellationToken cancellationToken)
    {
        var sourceRoot = await sourceDocument.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        if (sourceRoot is null) return solution;

        var newSourceInterface = sourceInterface.RemoveNode(methodDeclaration, SyntaxRemoveOptions.KeepNoTrivia);
        if (newSourceInterface is null) return solution;

        var newSourceRoot = sourceRoot.ReplaceNode(sourceInterface, newSourceInterface);
        return solution.WithDocumentSyntaxRoot(sourceDocument.Id, newSourceRoot);
    }

    private static async Task<Solution> AddMethodToTargetDocument(
        Solution solution,
        Document targetDocument,
        MethodDeclarationSyntax methodDeclaration,
        INamedTypeSymbol targetInterface,
        CancellationToken cancellationToken)
    {
        var updatedTargetDocument = solution.GetDocument(targetDocument.Id);
        if (updatedTargetDocument is null) return solution;

        var targetRoot = await updatedTargetDocument.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        if (targetRoot is null) return solution;

        var targetSemanticModel = await updatedTargetDocument.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
        if (targetSemanticModel is null) return solution;

        var targetInterfaceDeclaration = FindTargetInterfaceDeclaration(targetRoot, targetSemanticModel, targetInterface);
        if (targetInterfaceDeclaration is null) return solution;

        var cleanMethodDeclaration = CreateCleanMethodDeclaration(methodDeclaration);
        var newTargetInterface = targetInterfaceDeclaration.AddMembers(cleanMethodDeclaration);
        var newTargetRoot = targetRoot.ReplaceNode(targetInterfaceDeclaration, newTargetInterface);

        return solution.WithDocumentSyntaxRoot(updatedTargetDocument.Id, newTargetRoot);
    }

    private static InterfaceDeclarationSyntax? FindTargetInterfaceDeclaration(
        SyntaxNode root,
        SemanticModel semanticModel,
        INamedTypeSymbol targetInterface)
    {
        return root.DescendantNodes()
            .OfType<InterfaceDeclarationSyntax>()
            .FirstOrDefault(i =>
            {
                var symbol = semanticModel.GetDeclaredSymbol(i);
                return symbol?.Name == targetInterface.Name;
            });
    }

    private static MethodDeclarationSyntax CreateCleanMethodDeclaration(MethodDeclarationSyntax methodDeclaration)
    {
        return methodDeclaration
            .WithLeadingTrivia(SyntaxFactory.TriviaList(SyntaxFactory.Whitespace("    ")))
            .WithTrailingTrivia(SyntaxFactory.TriviaList(SyntaxFactory.LineFeed));
    }

    private static async Task<Document?> FindInterfaceDocumentAsync(
        Solution solution,
        INamedTypeSymbol interfaceSymbol,
        CancellationToken cancellationToken)
    {
        foreach (var project in solution.Projects)
        {
            foreach (var document in project.Documents)
            {
                if (await ContainsInterfaceAsync(document, interfaceSymbol, cancellationToken))
                {
                    return document;
                }
            }
        }

        return null;
    }

    private static async Task<bool> ContainsInterfaceAsync(
        Document document,
        INamedTypeSymbol interfaceSymbol,
        CancellationToken cancellationToken)
    {
        var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
        if (semanticModel is null) return false;

        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        if (root is null) return false;

        var interfaceDeclarations = root.DescendantNodes().OfType<InterfaceDeclarationSyntax>();
        
        foreach (var interfaceDecl in interfaceDeclarations)
        {
            var symbol = semanticModel.GetDeclaredSymbol(interfaceDecl);
            if (symbol?.Name == interfaceSymbol.Name &&
                symbol.ContainingNamespace.ToDisplayString() == interfaceSymbol.ContainingNamespace.ToDisplayString())
            {
                return true;
            }
        }

        return false;
    }

    private record CodeFixAnalysisResult(
        MethodDeclarationSyntax MethodDeclaration,
        InterfaceDeclarationSyntax InterfaceDeclaration,
        INamedTypeSymbol InterfaceSymbol,
        INamedTypeSymbol? ServerType,
        INamedTypeSymbol? ClientType);
}