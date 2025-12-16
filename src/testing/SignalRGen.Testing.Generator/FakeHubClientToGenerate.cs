using SignalRGen.Generator;
using SignalRGen.Generator.Common;

namespace SignalRGen.Testing.Generator;

internal sealed record FakeHubClientToGenerate(
    string Namespace,
    string HubClientName,
    EquatableArray<CacheableMethodDeclaration> ClientToServerMethods,
    EquatableArray<CacheableMethodDeclaration> ServerToClientMethods
);