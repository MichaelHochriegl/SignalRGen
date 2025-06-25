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
    private readonly INamedTypeSymbol _clientToServerAttribute;
    private readonly CancellationToken _cancellationToken;

    private readonly List<CacheableMethodDeclaration> _serverToClientMethods = [];
    private readonly List<CacheableMethodDeclaration> _clientToServerMethods = [];
    private readonly HashSet<string> _usings = [];
    private readonly HashSet<string> _methodSignatures = [];

    public InterfaceMethodExtractor(
        SemanticModel semanticModel,
        InterfaceDeclarationSyntax interfaceSyntax,
        INamedTypeSymbol interfaceSymbol,
        INamedTypeSymbol clientToServerAttribute,
        CancellationToken cancellationToken)
    {
        _semanticModel = semanticModel;
        _interfaceSyntax = interfaceSyntax;
        _interfaceSymbol = interfaceSymbol;
        _clientToServerAttribute = clientToServerAttribute;
        _cancellationToken = cancellationToken;
    }

    /// <summary>
    /// Extract all methods and usings from the interface and its base interfaces
    /// </summary>
    public ExtractedInterfaceData Extract()
    {
        // First, collect usings from the current interface
        CollectUsingsFromCurrentInterface();

        // Process methods from the current interface
        ProcessMethodsFromSyntax(_interfaceSyntax, _semanticModel);

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

            foreach (var syntaxRef in baseInterface.DeclaringSyntaxReferences)
            {
                var isPartOfCompilation = _semanticModel.Compilation.SyntaxTrees.Contains(syntaxRef.SyntaxTree);

                if (isPartOfCompilation &&
                    syntaxRef.GetSyntax(_cancellationToken) is InterfaceDeclarationSyntax baseInterfaceSyntax)
                {
                    // Get the semantic model for the syntax tree if different
                    var baseInterfaceModel = syntaxRef.SyntaxTree != _semanticModel.SyntaxTree
                        ? _semanticModel.Compilation.GetSemanticModel(syntaxRef.SyntaxTree)
                        : _semanticModel;

                    // Collect usings from the base interface
                    var compilationUnit = baseInterfaceSyntax.Ancestors().OfType<CompilationUnitSyntax>()
                        .FirstOrDefault();
                    if (compilationUnit != null)
                    {
                        foreach (var usingDirective in compilationUnit.Usings)
                        {
                            _usings.Add(usingDirective.ToString());
                        }
                    }

                    // Process methods from the base interface
                    ProcessMethodsFromSyntax(baseInterfaceSyntax, baseInterfaceModel);
                }
                else
                {
                    ProcessInterfaceFromMetadata(baseInterface);
                }
            }
        }
    }

    private void ProcessMethodsFromSyntax(
        InterfaceDeclarationSyntax interfaceSyntax,
        SemanticModel semanticModel)
    {
        foreach (var method in interfaceSyntax.Members.OfType<MethodDeclarationSyntax>())
        {
            _cancellationToken.ThrowIfCancellationRequested();

            // Create method declaration
            var methodDeclaration = new CacheableMethodDeclaration(
                method.Identifier.Text,
                method.ParameterList.Parameters
                    .Select(p => new Parameter(p.Type!.ToString(), p.Identifier.Text))
                    .ToImmutableArray(),
                method.ReturnType.ToString());

            // Check if we've already processed a method with this signature
            var signature = GetMethodSignature(methodDeclaration);
            if (!_methodSignatures.Add(signature))
            {
                // Skip duplicate methods
                continue;
            }

            // Check if method has ClientToServerMethodAttribute
            var isClientToServer = HasClientToServerAttribute(method, semanticModel);

            if (isClientToServer)
            {
                _clientToServerMethods.Add(methodDeclaration);
            }
            else
            {
                _serverToClientMethods.Add(methodDeclaration);
            }
        }
    }

    private bool HasClientToServerAttribute(MethodDeclarationSyntax method, SemanticModel semanticModel)
    {
        foreach (var attributeList in method.AttributeLists)
        {
            foreach (var attribute in attributeList.Attributes)
            {
                var attributeSymbol = semanticModel.GetSymbolInfo(attribute).Symbol;

                if (attributeSymbol is not null &&
                    attributeSymbol.ContainingType.Equals(_clientToServerAttribute, SymbolEqualityComparer.Default))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static string GetMethodSignature(CacheableMethodDeclaration method)
    {
        var parameterTypes = string.Join(",", method.Parameters.Select(p => p.Type));
        return $"{method.Identifier}({parameterTypes}):{method.ReturnType}";
    }

    private void ProcessInterfaceFromMetadata(INamedTypeSymbol interfaceSymbol)
    {
        // For cross-assembly interfaces, we work directly with symbols
        foreach (var member in interfaceSymbol.GetMembers())
        {
            if (member is IMethodSymbol method && method.MethodKind == MethodKind.Ordinary)
            {
                _cancellationToken.ThrowIfCancellationRequested();

                // Create method declaration from symbol
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

                // Check if method has ClientToServerMethodAttribute
                var isClientToServer = method.GetAttributes()
                    .Any(attr =>
                        attr.AttributeClass?.Equals(_clientToServerAttribute, SymbolEqualityComparer.Default) == true);

                if (isClientToServer)
                {
                    _clientToServerMethods.Add(methodDeclaration);
                }
                else
                {
                    _serverToClientMethods.Add(methodDeclaration);
                }

                // Add namespace usings for cross-assembly types
                AddNamespaceUsings(method);
            }
        }
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
            if (!parameter.Type.ContainingNamespace.IsGlobalNamespace)
            {
                _usings.Add($"using {parameter.Type.ContainingNamespace.ToDisplayString()};");
            }
        }
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