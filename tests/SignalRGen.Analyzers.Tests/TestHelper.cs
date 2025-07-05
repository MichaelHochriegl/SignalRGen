using Microsoft.AspNetCore.SignalR.Client;
using SignalRGen.Abstractions;

namespace SignalRGen.Analyzers.Tests;

public static class TestHelper
{
    public static CSharpAnalyzerTest<TAnalyzer, DefaultVerifier> CreateAnalyzerTest<TAnalyzer>()
        where TAnalyzer : DiagnosticAnalyzer, new()
    {
        var test = new CSharpAnalyzerTest<TAnalyzer, DefaultVerifier>
        {
            TestState =
            {
                ReferenceAssemblies = ReferenceAssemblies.Net.Net90,
                AdditionalReferences =
                {
                    // Add Microsoft.AspNetCore.SignalR.Core reference
                    MetadataReference.CreateFromFile(typeof(HubConnection).Assembly.Location),
                    MetadataReference.CreateFromFile(typeof(IBidirectionalHub<,>).Assembly.Location)
                }
            }
        };

        return test;
    }
}