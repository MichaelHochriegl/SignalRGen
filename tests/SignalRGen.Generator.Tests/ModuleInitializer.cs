using System.Runtime.CompilerServices;

namespace SignalRGen.Generator.Tests;

public static class ModuleInitializer
{
    [ModuleInitializer]
    public static void Init() => VerifySourceGenerators.Initialize();
}