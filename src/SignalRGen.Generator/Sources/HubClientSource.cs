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
            NamespaceName = hubClientToGenerate.InterfaceNamespace,
            HubName = hubClientToGenerate.HubName,
            HubUri = hubClientToGenerate.HubUri,
            HubClientInterface = hubClientToGenerate.InterfaceName,
            FullInterfaceName = fullInterfaceName,
            ServerToClientMethods = hubClientToGenerate.ServerToClientMethods.Select(method => new
            {
                Identifier = method.Identifier,
                Parameters = method.Parameters.Select(p => new
                {
                    Type = p.Type.Replace("*", ""),
                    Name = p.Name
                }).ToArray(),
                ParameterTypes = string.Join(", ", method.Parameters.Select(p => p.Type.Replace("*", ""))),
                ParameterList = string.Join(", ", method.Parameters.Select(p => $"{p.Type.Replace("*", "")} {p.Name}")),
                ParameterNames = string.Join(", ", method.Parameters.Select(p => p.Name)),
                HasParameters = method.Parameters.Any()
            }).ToArray(),
            ClientToServerMethods = hubClientToGenerate.ClientToServerMethods.Select(method => new
            {
                Identifier = method.Identifier,
                Parameters = method.Parameters.Select(p => new
                {
                    Type = p.Type,
                    Name = p.Name
                }).ToArray(),
                ParameterTypes = string.Join(", ", method.Parameters.Select(p => p.Type)),
                ParameterList = string.Join(", ", method.Parameters.Select(p => $"{p.Type} {p.Name}")),
                ParameterNames = string.Join(", ", method.Parameters.Select(p => p.Name)),
                HasParameters = method.Parameters.Any(),
                ReturnType = method.ReturnType,
                GenericReturnType = method.AwaitableReturnType is not null ? $"<{method.AwaitableReturnType}>" : string.Empty
            }).ToArray()
        };

        var template = TemplateLoader.GetTemplate("HubClient")
            .Render(model);

        return SourceText.From(template, Encoding.UTF8);
    }
}