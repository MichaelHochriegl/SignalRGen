using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SignalRGen.Generator.Common;

namespace SignalRGen.Generator.Extractors;

/// <summary>
/// Helper class for extracting methods from interfaces including base interfaces
/// </summary>
internal class InterfaceMethodExtractor
{
    private readonly SemanticModel _semanticModel;
    private readonly InterfaceDeclarationSyntax _interfaceSyntax;
    private readonly INamedTypeSymbol _interfaceSymbol;
    private readonly CancellationToken _cancellationToken;

    private readonly List<CacheableMethodDeclaration> _serverToClientMethods = [];
    private readonly List<CacheableMethodDeclaration> _clientToServerMethods = [];
    private readonly HashSet<string> _usings = [];
    private readonly HashSet<string> _methodSignatures = [];

    public InterfaceMethodExtractor(
        SemanticModel semanticModel,
        InterfaceDeclarationSyntax interfaceSyntax,
        INamedTypeSymbol interfaceSymbol,
        CancellationToken cancellationToken)
    {
        _semanticModel = semanticModel;
        _interfaceSyntax = interfaceSyntax;
        _interfaceSymbol = interfaceSymbol;
        _cancellationToken = cancellationToken;
    }

    /// <summary>
    /// Extract all methods and usings from the interface and its base interfaces
    /// </summary>
    public ExtractedInterfaceData Extract()
    {
        // First, collect usings from the current interface
        CollectUsingsFromCurrentInterface();

        // Process methods from base interfaces
        ProcessBaseInterfaces();

        return new ExtractedInterfaceData(
            serverToClientMethods: _serverToClientMethods.ToImmutableArray(),
            clientToServerMethods: _clientToServerMethods.ToImmutableArray(),
            usings: _usings.Select(u => new CacheableUsingDeclaration(u))
                .ToImmutableArray()
                .AsEquatableArray()
        );
    }

    private void CollectUsingsFromCurrentInterface()
    {
        // Find the compilation unit that contains the interface
        var compilationUnit = _interfaceSyntax.Ancestors().OfType<CompilationUnitSyntax>().FirstOrDefault();
        if (compilationUnit != null)
        {
            foreach (var usingDirective in compilationUnit.Usings)
            {
                _usings.Add(usingDirective.ToString());
            }
        }
    }

    private void ProcessBaseInterfaces()
    {
        foreach (var baseInterface in _interfaceSymbol.AllInterfaces)
        {
            _cancellationToken.ThrowIfCancellationRequested();

            // This has to work with magic strings, unfortunately, as `nameof` is not supported for generic types
            if (IsMarkerInterface(baseInterface, "IBidirectionalHub"))
            {
                // Extract methods from both generic parameters
                var typeArguments = baseInterface.TypeArguments;
                if (typeArguments.Length == 2)
                {
                    // First type argument is server-to-client methods
                    ProcessMethodsFromInterface(typeArguments[0], isServerToClient: true);
                    // Second type argument is client-to-server methods  
                    ProcessMethodsFromInterface(typeArguments[1], isServerToClient: false);
                }
            }
            else if (IsMarkerInterface(baseInterface, "IServerToClientHub"))
            {
                var typeArguments = baseInterface.TypeArguments;
                if (typeArguments.Length == 1)
                {
                    // Only have one type argument, so it's server-to-client methods
                    ProcessMethodsFromInterface(typeArguments[0], isServerToClient: true);
                }
            }
            else if (IsMarkerInterface(baseInterface, "IClientToServerHub"))
            {
                var typeArguments = baseInterface.TypeArguments;
                if (typeArguments.Length == 1)
                {
                    // Only have one type argument, so it's client-to-server methods
                    ProcessMethodsFromInterface(typeArguments[0], isServerToClient: false);
                }
            }
        }
    }
    
    private void ProcessMethodsFromInterface(ITypeSymbol typeSymbol, bool isServerToClient)
    {
        if (typeSymbol is not INamedTypeSymbol namedTypeSymbol) return;

        foreach (var member in namedTypeSymbol.GetMembers())
        {
            if (member is IMethodSymbol method && method.MethodKind == MethodKind.Ordinary)
            {
                _cancellationToken.ThrowIfCancellationRequested();

                var methodDeclaration = new CacheableMethodDeclaration(
                    method.Name,
                    method.Parameters
                        .Select(p => new Parameter(p.Type.ToDisplayString(), p.Name))
                        .ToImmutableArray(),
                    method.ReturnType.ToDisplayString());

                // Check if we've already processed a method with this signature
                var signature = GetMethodSignature(methodDeclaration);
                if (!_methodSignatures.Add(signature))
                {
                    continue;
                }

                if (isServerToClient)
                {
                    _serverToClientMethods.Add(methodDeclaration);
                }
                else
                {
                    _clientToServerMethods.Add(methodDeclaration);
                }

                // Add namespace usings
                AddNamespaceUsings(method);
            }
        }

        // Also process methods from base interfaces of the type argument
        foreach (var baseInterface in namedTypeSymbol.AllInterfaces)
        {
            ProcessMethodsFromInterface(baseInterface, isServerToClient);
        }
    }

    private static string GetMethodSignature(CacheableMethodDeclaration method)
    {
        var parameterTypes = string.Join(",", method.Parameters.Select(p => p.Type));
        return $"{method.Identifier}({parameterTypes}):{method.ReturnType}";
    }

    private void AddNamespaceUsings(IMethodSymbol method)
    {
        // Add using for return type
        if (!method.ReturnType.ContainingNamespace.IsGlobalNamespace)
        {
            _usings.Add($"using {method.ReturnType.ContainingNamespace.ToDisplayString()};");
        }

        // Add usings for parameter types
        foreach (var parameter in method.Parameters)
        {
            if (parameter.Type.ContainingNamespace is not null && !parameter.Type.ContainingNamespace.IsGlobalNamespace)
            {
                _usings.Add($"using {parameter.Type.ContainingNamespace.ToDisplayString()};");
            }
        }
    }
    
    private bool IsMarkerInterface(INamedTypeSymbol interfaceSymbol, string interfaceName)
    {
        return interfaceSymbol.Name == interfaceName && 
               interfaceSymbol.ContainingNamespace.ToDisplayString() == "SignalRGen.Abstractions";
    }

}

/// <summary>
/// Data extracted from an interface including its base interfaces
/// </summary>
internal class ExtractedInterfaceData(
    ImmutableArray<CacheableMethodDeclaration> serverToClientMethods,
    ImmutableArray<CacheableMethodDeclaration> clientToServerMethods,
    EquatableArray<CacheableUsingDeclaration> usings)
{
    public ImmutableArray<CacheableMethodDeclaration> ServerToClientMethods { get; } = serverToClientMethods;
    public ImmutableArray<CacheableMethodDeclaration> ClientToServerMethods { get; } = clientToServerMethods;
    public EquatableArray<CacheableUsingDeclaration> Usings { get; } = usings;
}