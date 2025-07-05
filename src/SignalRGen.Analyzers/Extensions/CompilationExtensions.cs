using Microsoft.CodeAnalysis;

namespace SignalRGen.Analyzers.Extensions;

internal static class CompilationExtensions
{
    public static IEnumerable<INamedTypeSymbol> GetAllNamedTypes(this Compilation compilation, CancellationToken ct)
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
}