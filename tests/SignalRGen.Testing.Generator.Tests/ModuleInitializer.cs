using System.Runtime.CompilerServices;

namespace SignalRGen.Generator.Tests;

public static class ModuleInitializer
{
    [ModuleInitializer]
    public static void Init()
    {
        DerivePathInfo(
            (sourceFile, projectDirectory, type, method) => new(
                directory: Path.Combine(projectDirectory, "Snapshots", type.Name),
                typeName: type.Name,
                methodName: method.Name));
        
        VerifySourceGenerators.Initialize();
    }
}