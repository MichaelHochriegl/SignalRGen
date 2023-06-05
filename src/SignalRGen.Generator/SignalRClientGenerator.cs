using Microsoft.CodeAnalysis;
using SignalRGen.Generator.Sources;

namespace SignalRGen.Generator;

[Generator]
internal sealed class SignalRClientGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(ctx => ctx.AddSource("HubClientAttribute.g.cs", HubClientAttributeSource.GetSource()));
    }
}