namespace SignalRGen.Generator.Common;

public record CacheableUsingDeclaration(string UsingNamespace)
{
    public string UsingNamespace { get; } = UsingNamespace;
}