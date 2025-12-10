using System.Collections.Immutable;
using SignalRGen.Generator.Common;

namespace SignalRGen.Generator.Extractors;

/// <summary>
/// Data extracted from an interface including its base interfaces
/// </summary>
internal class ExtractedInterfaceData(
    ImmutableArray<CacheableMethodDeclaration> serverToClientMethods,
    ImmutableArray<CacheableMethodDeclaration> clientToServerMethods)
{
    public ImmutableArray<CacheableMethodDeclaration> ServerToClientMethods { get; } = serverToClientMethods;
    public ImmutableArray<CacheableMethodDeclaration> ClientToServerMethods { get; } = clientToServerMethods;
}