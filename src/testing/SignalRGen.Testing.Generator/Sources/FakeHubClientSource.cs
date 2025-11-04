// C#

using System.Text;
using Microsoft.CodeAnalysis.Text;
using Scriban;
using Scriban.Runtime;
using SignalRGen.Generator.Common;
using SignalRGen.Shared.Common;

namespace SignalRGen.Testing.Generator.Sources;

internal static class FakeHubClientSource
{
    internal static SourceText GetSource(FakeHubClientToGenerate model)
    {
        var methodScriptObject = new ScriptObject();
        methodScriptObject.Import(typeof(CaseUtil));

        var modelScriptObject = new ScriptObject
        {
            { "namespace_name", model.Namespace },
            { "hubClient_name", model.HubClientName },
            {
                "client_to_server", model.ClientToServerMethods.Select(m => new
                {
                    identifier = m.Identifier,
                    parameters = m.Parameters.Select(p => new { type = p.Type, name = p.Name }).ToArray(),
                    returnType = m.ReturnType,
                    awaitableReturnType = m.AwaitableReturnType
                }).ToArray()
            },
            {
                "server_to_client", model.ServerToClientMethods.Select(m => new
                {
                    identifier = m.Identifier,
                    parameters = m.Parameters.Select(p => new { type = p.Type, name = p.Name }).ToArray()
                }).ToArray()
            }
        };

        var context = new TemplateContext();
        context.PushGlobal(methodScriptObject);
        context.PushGlobal(modelScriptObject);
        
        var template = TemplateLoader.GetTemplate("FakeHubClient")
            .Render(context);

        return SourceText.From(template, Encoding.UTF8);
    }
}