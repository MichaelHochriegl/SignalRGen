using Microsoft.CodeAnalysis.CSharp.Syntax;
using SignalRGen.Generator.Common;

namespace SignalRGen.Generator;

internal sealed record HubClientToGenerate(
    string InterfaceName,
    string HubName,
    string HubUri,
    string InterfaceNamespace,
    EquatableArray<CacheableMethodDeclaration> ServerToClientMethods,
    EquatableArray<CacheableMethodDeclaration> ClientToServerMethods,
    EquatableArray<CacheableUsingDeclaration> Usings)
{
    public string InterfaceName { get; } = InterfaceName;
    public string HubName { get; } = HubName;
    public string HubUri { get; } = HubUri;
    public string InterfaceNamespace { get; } = InterfaceNamespace;
    public EquatableArray<CacheableMethodDeclaration> ServerToClientMethods { get; } = ServerToClientMethods;
    public EquatableArray<CacheableMethodDeclaration> ClientToServerMethods { get; } = ClientToServerMethods;
    public EquatableArray<CacheableUsingDeclaration> Usings { get; } = Usings;
}

