using System.Collections.Immutable;
using System.Composition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SignalRGen.Shared;

namespace SignalRGen.CodeFixes;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ServerToClientReturnTypeCodeFixProvider)), Shared]
public class ServerToClientReturnTypeCodeFixProvider : CodeFixProvider
{
    public sealed override ImmutableArray<string> FixableDiagnosticIds =>
        [DiagnosticIds.SRG0002OnlyTaskMethodsAllowedInServerToClientHubContract];

    public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        if (root is null) return;

        var semanticModel = await context.Document.GetSemanticModelAsync(context.CancellationToken).ConfigureAwait(false);
        if (semanticModel is null) return;
        
        var diagnostic = context.Diagnostics.FirstOrDefault(d => d.Id == DiagnosticIds.SRG0002OnlyTaskMethodsAllowedInServerToClientHubContract);
        if (diagnostic is null) return;
        
        var diagnosticSpan = diagnostic.Location.SourceSpan;
        var returnTypeNode = root.FindNode(diagnosticSpan);
            
        var methodDeclaration = returnTypeNode.Ancestors().OfType<MethodDeclarationSyntax>().FirstOrDefault();
        if (methodDeclaration is null) return;
        
        var typeInfo = semanticModel.GetTypeInfo(methodDeclaration.ReturnType, context.CancellationToken);
        var returnType = typeInfo.Type;
        
        if (returnType is null || IsValidTaskType(returnType)) return;

        var action = CodeAction.Create(
            title: "Change return type to Task",
            createChangedDocument: c => ConvertToTask(context.Document, methodDeclaration, c),
            equivalenceKey: nameof(ServerToClientReturnTypeCodeFixProvider));

        context.RegisterCodeFix(action, diagnostic);
    }

    private static async Task<Document> ConvertToTask(
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

    private static bool TryAddTaskUsing(SyntaxNode root, out SyntaxNode newRoot)
    {
        var compilationUnit = root as CompilationUnitSyntax;
        var hasTaskUsing = compilationUnit?.Usings.Any(u =>
            u.Name?.ToString() == "System.Threading.Tasks") ?? false;

        if (!hasTaskUsing && compilationUnit != null)
        {
            var usingDirective = SyntaxFactory.UsingDirective(
                SyntaxFactory.QualifiedName(
                    SyntaxFactory.QualifiedName(
                        SyntaxFactory.IdentifierName("System"),
                        SyntaxFactory.IdentifierName("Threading")),
                    SyntaxFactory.IdentifierName("Tasks")));
            newRoot = compilationUnit.AddUsings(usingDirective);
            return true;
        }

        newRoot = root;
        return false;
    }

    private static bool IsValidTaskType(ITypeSymbol returnType)
    {
        return returnType is INamedTypeSymbol namedType &&
               namedType.Name == "Task" &&
               namedType.ContainingNamespace.ToDisplayString() == "System.Threading.Tasks" &&
               namedType.TypeArguments.Length == 0;
    }
}