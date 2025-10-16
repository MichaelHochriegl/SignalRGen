using System.Text;
using Microsoft.CodeAnalysis.Text;
using SignalRGen.Generator.Common;

namespace SignalRGen.Generator.Sources;

internal static class HubClientSource
{
    internal static SourceText GetSourceText(HubClientToGenerate hubClientToGenerate)
    {
        var fullInterfaceName = $"global::{hubClientToGenerate.InterfaceNamespace}.{hubClientToGenerate.InterfaceName}";
        
        var model = new
        {
            namespace_name = hubClientToGenerate.InterfaceNamespace,
            hub_name = hubClientToGenerate.HubName,
            hub_uri = hubClientToGenerate.HubUri,
            hub_client_interface = hubClientToGenerate.InterfaceName,
            full_interface_name = fullInterfaceName,
            server_to_client_methods = hubClientToGenerate.ServerToClientMethods.Select(method => new
            {
                identifier = method.Identifier,
                parameters = method.Parameters.Select(p => new
                {
                    type = p.Type.Replace("*", ""),
                    name = p.Name
                }).ToArray(),
                parameter_types = string.Join(", ", method.Parameters.Select(p => p.Type.Replace("*", ""))),
                parameter_list = string.Join(", ", method.Parameters.Select(p => $"{p.Type.Replace("*", "")} {p.Name}")),
                parameter_names = string.Join(", ", method.Parameters.Select(p => p.Name)),
                has_parameters = method.Parameters.Any()
            }).ToArray(),
            client_to_server_methods = hubClientToGenerate.ClientToServerMethods.Select(method => new
            {
                identifier = method.Identifier,
                parameters = method.Parameters.Select(p => new
                {
                    type = p.Type,
                    name = p.Name
                }).ToArray(),
                parameter_types = string.Join(", ", method.Parameters.Select(p => p.Type)),
                parameter_list = string.Join(", ", method.Parameters.Select(p => $"{p.Type} {p.Name}")),
                parameter_names = string.Join(", ", method.Parameters.Select(p => p.Name)),
                has_parameters = method.Parameters.Any(),
                return_type = method.ReturnType,
                generic_return_type = method.AwaitableReturnType is not null ? $"<{method.AwaitableReturnType}>" : string.Empty
            }).ToArray()
        };

        var template = TemplateLoader.GetTemplate("HubClient")
            .Render(model);

        return SourceText.From(template, Encoding.UTF8);
    }
}