using System.Runtime.CompilerServices;
using VerifyTests;
using VerifyXunit;

namespace SignalRGen.Testing.Generator.Tests;

public static class ModuleInitializer
{
    [ModuleInitializer]
    public static void Init()
    {
        Verifier.DerivePathInfo(
            (_, projectDirectory, type, method) => new(
                directory: Path.Combine(projectDirectory, "Snapshots", type.Name),
                typeName: type.Name,
                methodName: method.Name));
        
        VerifySourceGenerators.Initialize();
    }
}