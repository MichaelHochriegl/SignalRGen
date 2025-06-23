namespace SignalRGen.Generator.Common;

internal class MsBuildOptions(string rootNamespace)
{
    public string RootNamespace { get; } = rootNamespace;
}