namespace SignalRGen.Generator.Common;

internal sealed record CacheableMethodDeclaration(string Identifier, EquatableArray<Parameter> Parameters, string ReturnType, string? AwaitableReturnType)
{
    public string Identifier { get; } = Identifier;
    public EquatableArray<Parameter> Parameters { get; } = Parameters;

    public string ReturnType { get; } = ReturnType;
    public string? AwaitableReturnType { get; } = AwaitableReturnType;
}

internal sealed record Parameter(string Type, string Name)
{
    public string Type { get; } = Type;
    public string Name { get; } = Name;
}