using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace SignalRGen.Analyzers.Extensions;

internal static class SymbolExtensions
{
    /// <summary>
    /// Retrieves all methods declared within the provided interface as well as all methods from its inherited interfaces, recursively.
    /// </summary>
    /// <param name="interfaceSymbol">The interface symbol representing the interface to retrieve methods from.</param>
    /// <returns>An immutable array of method symbols, containing all methods from the interface and its base interfaces.</returns>
    public static ImmutableArray<IMethodSymbol> GetInterfaceMethodsRecursively(this INamedTypeSymbol interfaceSymbol)
    {
        var methods = new List<IMethodSymbol>();
        var visited = new HashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default);

        CollectMethodsRecursively(interfaceSymbol, methods, visited);

        return [..methods];
    }

    private static void CollectMethodsRecursively(INamedTypeSymbol interfaceSymbol, List<IMethodSymbol> methods,
        HashSet<INamedTypeSymbol> visited)
    {
        if (!visited.Add(interfaceSymbol)) return;
        
        foreach (var member in interfaceSymbol.GetMembers())
        {
            if (member is IMethodSymbol methodSymbol)
            {
                methods.Add(methodSymbol);
            }
            else if (member is INamedTypeSymbol namedTypeSymbol)
            {
                CollectMethodsRecursively(namedTypeSymbol, methods, visited);
            }
        }

        foreach (var baseInterface in interfaceSymbol.AllInterfaces)
        {
            CollectMethodsRecursively(baseInterface, methods, visited);
        }
    }
}