using System.Collections.Immutable;
using System.Composition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SignalRGen.Analyzers;

namespace SignalRGen.CodeFixes;

[ExportCodeFixProvider(LanguageNames.CSharp, nameof(ClientToServerReturnTypeCodeFixProvider)), Shared]
public class ClientToServerReturnTypeCodeFixProvider : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds { get; } =
    [
        DiagnosticIds.SRG0003OnlyTaskOrTaskOfTMethodsAllowedInClientToServerHubContract
    ];

    public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        if (root is null) return;

        var semanticModel =
            await context.Document.GetSemanticModelAsync(context.CancellationToken).ConfigureAwait(false);
        if (semanticModel is null) return;

        // Get the specific diagnostic you care about
        var diagnostic = context.Diagnostics.FirstOrDefault(d =>
            d.Id == DiagnosticIds.SRG0003OnlyTaskOrTaskOfTMethodsAllowedInClientToServerHubContract);

        if (diagnostic is null) return;

        var diagnosticSpan = diagnostic.Location.SourceSpan;
        var returnTypeNode = root.FindNode(diagnosticSpan);

        var methodDeclaration = returnTypeNode.Ancestors().OfType<MethodDeclarationSyntax>().FirstOrDefault();
        if (methodDeclaration is null) return;

        // Determine what type of fix to offer based on the return type
        var typeInfo = semanticModel.GetTypeInfo(methodDeclaration.ReturnType, context.CancellationToken);
        var returnType = typeInfo.Type;

        if (returnType is null || IsTaskType(returnType)) return;

        CodeAction action;

        if (returnType.SpecialType == SpecialType.System_Void)
        {
            // void → Task
            action = CodeAction.Create(
                title: "Change return type to Task",
                createChangedDocument: c => ConvertVoidToTask(context.Document, methodDeclaration, c),
                equivalenceKey: $"{nameof(ClientToServerReturnTypeCodeFixProvider)}-VoidToTask");
        }
        else
        {
            // T → Task<T>
            action = CodeAction.Create(
                title: $"Change return type to Task<{returnTypeNode}>",
                createChangedDocument: c => ConvertToTaskT(context.Document, methodDeclaration, c),
                equivalenceKey: $"{nameof(ClientToServerReturnTypeCodeFixProvider)}-ToTaskT");
        }

        context.RegisterCodeFix(action, diagnostic);
    }

    private async Task<Document> ConvertVoidToTask(
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
        
        var wasUpdated = TryAddTaskUsing(newRoot, out var updatedRoot);

        return document.WithSyntaxRoot(wasUpdated ? updatedRoot : newRoot);
    }

    private static async Task<Document> ConvertToTaskT(
        Document document,
        MethodDeclarationSyntax methodDeclaration,
        CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        if (root is null) return document;
        
        var taskType = SyntaxFactory.IdentifierName("Task");
        var typeArgumentList = SyntaxFactory.TypeArgumentList(
            SyntaxFactory.SingletonSeparatedList(methodDeclaration.ReturnType));
        var newReturnType = SyntaxFactory.GenericName(taskType.Identifier, typeArgumentList)
            .WithTriviaFrom(methodDeclaration.ReturnType);

        var newMethodDeclaration = methodDeclaration.WithReturnType(newReturnType);
        var newRoot = root.ReplaceNode(methodDeclaration, newMethodDeclaration);

        var wasUpdated = TryAddTaskUsing(newRoot, out var updatedRoot);

        return document.WithSyntaxRoot(wasUpdated ? updatedRoot : newRoot);
    }
    
    private static bool TryAddTaskUsing(SyntaxNode root, out SyntaxNode newRoot)
    {
        var compilationUnit = root as CompilationUnitSyntax;
        var hasTaskUsing = compilationUnit?.Usings.Any(u =>
            u.Name?.ToString() == "System.Threading.Tasks") ?? false;
        
        if (!hasTaskUsing && compilationUnit != null)
        {
            var usingDirective = SyntaxFactory.UsingDirective(
                SyntaxFactory.IdentifierName("System.Threading.Tasks"));
            newRoot = ((CompilationUnitSyntax)root).AddUsings(usingDirective);
        }

        newRoot = root;
        return !hasTaskUsing;
    }
    
    private static bool IsTaskType(ITypeSymbol returnType)
    {
        return returnType is INamedTypeSymbol namedType &&
               namedType.Name == "Task" &&
               namedType.ContainingNamespace.ToDisplayString() == "System.Threading.Tasks";
    }
}