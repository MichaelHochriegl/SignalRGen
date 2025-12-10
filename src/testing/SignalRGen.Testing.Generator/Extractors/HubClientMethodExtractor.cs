using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using SignalRGen.Generator;
using SignalRGen.Generator.Common;
using SignalRGen.Generator.Extractors;
using SignalRGen.Shared.Extensions;

namespace SignalRGen.Testing.Generator.Extractors;

internal sealed class HubClientMethodExtractor
{
    private readonly SemanticModel _semanticModel;
    private readonly INamedTypeSymbol _hubClientTypeSymbol;

    public HubClientMethodExtractor(SemanticModel semanticModel, INamedTypeSymbol hubClientTypeSymbol)
    {
        _semanticModel = semanticModel;
        _hubClientTypeSymbol = hubClientTypeSymbol;
    }

    public ExtractedInterfaceData Extract()
    {
        var clientMethods = ExtractClientMethods();
        var serverMethods = ExtractServerMethods();
        
        return new ExtractedInterfaceData(serverMethods, clientMethods);
    }

    // TODO: Optimize this, as currently the methods are looped twice, once for client and once for server
    private EquatableArray<CacheableMethodDeclaration> ExtractClientMethods()
    {
        var methods = new List<CacheableMethodDeclaration>();

        foreach (var member in _hubClientTypeSymbol.GetMembers())
        {
            if (member is not IMethodSymbol methodSymbol)
            {
                continue;
            }

            if (methodSymbol.DeclaredAccessibility != Accessibility.Public)
            {
                continue;
            }
            
            if (!methodSymbol.Name.StartsWith("Invoke", StringComparison.Ordinal))
            {
                continue;
            }
            var nameSpan = methodSymbol.Name.AsSpan();
            var methodName = methodSymbol.Name
                .Replace("Invoke", string.Empty)
                .Replace("Async", string.Empty);
            
            var parameters = methodSymbol.Parameters
                .Where(p => p.Type
                    .ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) 
                            != "global::System.Threading.CancellationToken")
                .Select(p => new Parameter(p.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat), p.Name))
                .ToImmutableArray()
                .AsEquatableArray();
            var returnType = methodSymbol.ReturnType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            var awaitableReturnType = methodSymbol.ReturnType
                .GetAwaitableResultType(_semanticModel.Compilation)?
                .ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            
            methods.Add(new CacheableMethodDeclaration(methodName, parameters, returnType, awaitableReturnType));
        }
        
        return methods.ToImmutableArray();
    }
    
    private EquatableArray<CacheableMethodDeclaration> ExtractServerMethods()
    {
        var methods = new List<CacheableMethodDeclaration>();

        foreach (var member in _hubClientTypeSymbol.GetMembers())
        {
            if (member is not IFieldSymbol fieldSymbol)
            {
                continue;
            }

            if (fieldSymbol.DeclaredAccessibility != Accessibility.Public)
            {
                continue;
            }
            
            if (!fieldSymbol.Name.StartsWith("On", StringComparison.Ordinal))
            {
                continue;
            }
            var methodName = member.Name;

            var invokeMethod = (fieldSymbol.Type as INamedTypeSymbol)?.DelegateInvokeMethod;
            if (invokeMethod is null)
            {
                continue;
            }
            
            var parameters = invokeMethod.Parameters
                .Where(p => p.Type
                                .ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) 
                            != "global::System.Threading.CancellationToken")
                .Select(p => new Parameter(p.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat), p.Name))
                .ToImmutableArray()
                .AsEquatableArray();
            var returnType = invokeMethod.ReturnType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            var awaitableReturnType = invokeMethod.ReturnType
                .GetAwaitableResultType(_semanticModel.Compilation)?
                .ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            
            methods.Add(new CacheableMethodDeclaration(methodName, parameters, returnType, awaitableReturnType));
        }
        
        return methods.ToImmutableArray();
    }
}