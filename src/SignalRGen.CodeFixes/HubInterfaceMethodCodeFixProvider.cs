using System.Collections.Immutable;
using System.Composition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SignalRGen.Analyzers;

namespace SignalRGen.CodeFixes;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(HubInterfaceMethodCodeFixProvider)), Shared]
public class HubInterfaceMethodCodeFixProvider : CodeFixProvider
{
    public sealed override ImmutableArray<string> FixableDiagnosticIds =>
        [DiagnosticIds.SRG0001NoMethodsInHubContractAllowed];

    public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        if (root is null) return;
        
        var diagnostic =  context.Diagnostics.FirstOrDefault(d => d.Id == DiagnosticIds.SRG0001NoMethodsInHubContractAllowed);
        if (diagnostic is null) return;

        var diagnosticSpan = diagnostic.Location.SourceSpan;
        var methodDeclaration = root.FindToken(diagnosticSpan.Start).Parent?.AncestorsAndSelf()
            .OfType<MethodDeclarationSyntax>().FirstOrDefault();

        if (methodDeclaration is null) return;

        var interfaceDeclaration =
            methodDeclaration.Ancestors().OfType<InterfaceDeclarationSyntax>().FirstOrDefault();
        if (interfaceDeclaration is null) return;

        var semanticModel = await context.Document.GetSemanticModelAsync(context.CancellationToken)
            .ConfigureAwait(false);
        if (semanticModel is null) return;

        var interfaceSymbol = ModelExtensions.GetDeclaredSymbol(semanticModel, interfaceDeclaration) as INamedTypeSymbol;
        if (interfaceSymbol is null) return;

        var hubInterface = FindHubInterface(interfaceSymbol);
        if (hubInterface is null) return;

        var (serverType, clientType) = ExtractGenericTypes(hubInterface);

        var baseKey =
            $"{interfaceSymbol.ToDisplayString()}_{methodDeclaration.Identifier.ValueText}_{methodDeclaration.Span.Start}";

        if (serverType is not null)
        {
            var moveToServerAction = CodeAction.Create(
                title: $"Move to server interface ({serverType.Name})",
                createChangedSolution: c =>
                    MoveMethodToInterface(context.Document, methodDeclaration, interfaceDeclaration, serverType, c),
                equivalenceKey: nameof(HubInterfaceMethodCodeFixProvider) + "_" +
                                $"{baseKey}_Server_{serverType.ToDisplayString()}");

            context.RegisterCodeFix(moveToServerAction, diagnostic);
        }

        if (clientType is not null)
        {
            var moveToClientAction = CodeAction.Create(
                title: $"Move to client interface ({clientType.Name})",
                createChangedSolution: c =>
                    MoveMethodToInterface(context.Document, methodDeclaration, interfaceDeclaration, clientType, c),
                equivalenceKey: nameof(HubInterfaceMethodCodeFixProvider) + "_" +
                                $"{baseKey}_Client_{clientType.ToDisplayString()}");

            context.RegisterCodeFix(moveToClientAction, diagnostic);
        }
    }


    private static INamedTypeSymbol? FindHubInterface(INamedTypeSymbol interfaceSymbol)
    {
        return interfaceSymbol.AllInterfaces.FirstOrDefault(i =>
            i.Name == "IBidirectionalHub" ||
            i.Name == "IServerToClientHub" ||
            i.Name == "IClientToServerHub");
    }

    private static (INamedTypeSymbol? serverType, INamedTypeSymbol? clientType) ExtractGenericTypes(
        INamedTypeSymbol hubInterface)
    {
        if (hubInterface is { Name: "IBidirectionalHub", TypeArguments.Length: 2 })
        {
            return (hubInterface.TypeArguments[0] as INamedTypeSymbol,
                hubInterface.TypeArguments[1] as INamedTypeSymbol);
        }

        if (hubInterface is { Name: "IServerToClientHub", TypeArguments.Length: 1 })
        {
            return (hubInterface.TypeArguments[0] as INamedTypeSymbol, null);
        }

        if (hubInterface is { Name: "IClientToServerHub", TypeArguments.Length: 1 })
        {
            return (null, hubInterface.TypeArguments[0] as INamedTypeSymbol);
        }

        return (null, null);
    }


    private static async Task<Solution> MoveMethodToInterface(
        Document document,
        MethodDeclarationSyntax methodDeclaration,
        InterfaceDeclarationSyntax sourceInterface,
        INamedTypeSymbol targetInterface,
        CancellationToken cancellationToken)
    {
        var solution = document.Project.Solution;

        // Find the target interface document
        var targetDocument = FindInterfaceDocument(solution, targetInterface);
        if (targetDocument == null) return solution;

        // Handle same document scenario
        if (document.Id == targetDocument.Id)
        {
            return await MoveBothInterfacesInSameDocument(document, methodDeclaration, sourceInterface, targetInterface,
                cancellationToken);
        }

        // Handle cross-document scenario
        return await MoveBetweenDifferentDocuments(document, targetDocument, methodDeclaration, sourceInterface,
            targetInterface, cancellationToken);
    }


    private static async Task<Solution> MoveBothInterfacesInSameDocument(
        Document document,
        MethodDeclarationSyntax methodDeclaration,
        InterfaceDeclarationSyntax sourceInterface,
        INamedTypeSymbol targetInterface,
        CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        if (root == null) return document.Project.Solution;

        var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
        if (semanticModel == null) return document.Project.Solution;

        // Find target interface in the same document
        var targetInterfaceDeclaration = root.DescendantNodes()
            .OfType<InterfaceDeclarationSyntax>()
            .FirstOrDefault(i =>
            {
                var symbol = semanticModel.GetDeclaredSymbol(i);
                return symbol?.Name == targetInterface.Name;
            });

        if (targetInterfaceDeclaration == null) return document.Project.Solution;

        // Create clean method declaration
        var cleanMethodDeclaration = methodDeclaration
            .WithLeadingTrivia(SyntaxFactory.TriviaList(SyntaxFactory.Whitespace("    ")))
            .WithTrailingTrivia(SyntaxFactory.TriviaList(SyntaxFactory.ElasticCarriageReturnLineFeed));

        // Create updated interfaces
        var newSourceInterface = sourceInterface.RemoveNode(methodDeclaration, SyntaxRemoveOptions.KeepNoTrivia);
        if (newSourceInterface == null) return document.Project.Solution;

        var newTargetInterface = targetInterfaceDeclaration.AddMembers(cleanMethodDeclaration);

        // Use ReplaceNodes to replace both interfaces atomically
        var nodesToReplace = new Dictionary<SyntaxNode, SyntaxNode>
        {
            { sourceInterface, newSourceInterface },
            { targetInterfaceDeclaration, newTargetInterface }
        };

        var newRoot = root.ReplaceNodes(nodesToReplace.Keys, (original, _) => nodesToReplace[original]);

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

        // Remove method from source document
        var sourceRoot = await sourceDocument.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        if (sourceRoot == null) return solution;

        var newSourceInterface = sourceInterface.RemoveNode(methodDeclaration, SyntaxRemoveOptions.KeepNoTrivia);
        if (newSourceInterface == null) return solution;

        var newSourceRoot = sourceRoot.ReplaceNode(sourceInterface, newSourceInterface);
        solution = solution.WithDocumentSyntaxRoot(sourceDocument.Id, newSourceRoot);

        // Add method to target document (using updated solution)
        var updatedTargetDocument = solution.GetDocument(targetDocument.Id);
        if (updatedTargetDocument == null) return solution;

        var targetRoot = await updatedTargetDocument.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        if (targetRoot == null) return solution;

        var targetSemanticModel =
            await updatedTargetDocument.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
        if (targetSemanticModel == null) return solution;

        var targetInterfaceDeclaration = targetRoot.DescendantNodes()
            .OfType<InterfaceDeclarationSyntax>()
            .FirstOrDefault(i =>
            {
                var symbol = targetSemanticModel.GetDeclaredSymbol(i);
                return symbol?.Name == targetInterface.Name;
            });

        if (targetInterfaceDeclaration == null) return solution;

        // Create clean method declaration
        var cleanMethodDeclaration = methodDeclaration
            .WithLeadingTrivia(SyntaxFactory.TriviaList(SyntaxFactory.Whitespace("    ")))
            .WithTrailingTrivia(SyntaxFactory.TriviaList(SyntaxFactory.ElasticCarriageReturnLineFeed));

        var newTargetInterface = targetInterfaceDeclaration.AddMembers(cleanMethodDeclaration);
        var newTargetRoot = targetRoot.ReplaceNode(targetInterfaceDeclaration, newTargetInterface);

        return solution.WithDocumentSyntaxRoot(updatedTargetDocument.Id, newTargetRoot);
    }

    private static Document? FindInterfaceDocument(Solution solution, INamedTypeSymbol interfaceSymbol)
    {
        foreach (var project in solution.Projects)
        {
            foreach (var document in project.Documents)
            {
                var semanticModel = document.GetSemanticModelAsync().Result;
                if (semanticModel == null) continue;

                var root = document.GetSyntaxRootAsync().Result;
                if (root == null) continue;

                var interfaceDeclarations = root.DescendantNodes().OfType<InterfaceDeclarationSyntax>();
                foreach (var interfaceDecl in interfaceDeclarations)
                {
                    var symbol = semanticModel.GetDeclaredSymbol(interfaceDecl);
                    if (symbol?.Name == interfaceSymbol.Name &&
                        symbol.ContainingNamespace.ToDisplayString() ==
                        interfaceSymbol.ContainingNamespace.ToDisplayString())
                    {
                        return document;
                    }
                }
            }
        }

        return null;
    }
}