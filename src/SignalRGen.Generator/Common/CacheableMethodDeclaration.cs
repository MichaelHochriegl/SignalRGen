namespace SignalRGen.Generator.Common;

internal sealed record CacheableMethodDeclaration(string Identifier, EquatableArray<Parameter> Parameters)
{
    public string Identifier { get; } = Identifier;
    public EquatableArray<Parameter> Parameters { get; } = Parameters;
}

internal sealed record Parameter(string Type, string Name)
{
    public string Type { get; } = Type;
    public string Name { get; } = Name;
}