using Microsoft.CodeAnalysis;

namespace SignalRGen.Shared.Extensions;

public static class TypeSymbolExtensions
{
    public static ITypeSymbol? GetAwaitableResultType(this ITypeSymbol type, Compilation compilation)
    {
        if (type is INamedTypeSymbol named && named.IsGenericType)
        {
            var taskOfT = compilation.GetTypeByMetadataName("System.Threading.Tasks.Task`1");
            var valueTaskOfT = compilation.GetTypeByMetadataName("System.Threading.Tasks.ValueTask`1");

            if ((taskOfT is not null && SymbolEqualityComparer.Default.Equals(named.ConstructedFrom, taskOfT)) ||
                (valueTaskOfT is not null && SymbolEqualityComparer.Default.Equals(named.ConstructedFrom, valueTaskOfT)))
            {
                return named.TypeArguments[0];
            }
        }

        return null;
    }
}