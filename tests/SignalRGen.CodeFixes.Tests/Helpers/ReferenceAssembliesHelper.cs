using Microsoft.CodeAnalysis.Testing;

namespace SignalRGen.CodeFixes.Tests.Helpers;

internal static class ReferenceAssembliesHelper
{
    public static ReferenceAssemblies ForCurrentNet() =>
        Environment.Version.Major switch
        {
            8 => ReferenceAssemblies.Net.Net80,
            9 => ReferenceAssemblies.Net.Net90,
            10 => Net100,
            _ => ReferenceAssemblies.Net.Net80
        };
    
    // This is a really ugly hack, but mandatory for now until the Microsoft.CodeAnalysis.Testing is updated
    // GitHub issue: https://github.com/dotnet/roslyn-sdk/issues/1233
    private static readonly ReferenceAssemblies Net100 = new(
        targetFramework: "net10.0",
        referenceAssemblyPackage: new PackageIdentity("Microsoft.NETCore.App.Ref", "10.0.1"),
        referenceAssemblyPath: Path.Combine("ref", "net10.0"));
}

