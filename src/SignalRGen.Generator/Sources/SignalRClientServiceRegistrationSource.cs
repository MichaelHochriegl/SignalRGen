using System.Text;
using Microsoft.CodeAnalysis.Text;
using SignalRGen.Generator.Common;

namespace SignalRGen.Generator.Sources;

internal static class SignalRClientServiceRegistrationSource
{
    internal static SourceText GetSource(EquatableArray<HubClientToGenerate> hubs, MsBuildOptions? options)
    {
        var rootNamespace = options?.RootNamespace ?? "SignalRGen.Generator";

        var model = new
        {
            NamespaceName = rootNamespace.EndsWith(".Client.Extensions.DependencyInjection")
                ? rootNamespace
                : $"{rootNamespace}.Client.Extensions.DependencyInjection",
            ModuleName = options?.ModuleName ?? "SignalR",
            Selector = $"{rootNamespace}.HubClientBase",
            Hubs = hubs.Select(hub => new { HubName = hub.HubName }).ToArray()
        };
        
        var template = TemplateLoader.GetTemplate("SignalRClientServiceRegistration")
            .Render(model);

        return SourceText.From(template, Encoding.UTF8);
    }
}