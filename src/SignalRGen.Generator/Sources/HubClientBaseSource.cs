using System.Text;
using Microsoft.CodeAnalysis.Text;
using SignalRGen.Generator.Common;

namespace SignalRGen.Generator.Sources;

internal static class HubClientBaseSource
{
    internal static SourceText GetSource(MsBuildOptions? options)
    {
        var template = TemplateLoader.GetTemplate("HubClientBase")
            .Render(new
            {
                Namespace = options?.RootNamespace ?? "SignalRGen.Generator"
            });

        return SourceText.From(template, Encoding.UTF8);
    }
}