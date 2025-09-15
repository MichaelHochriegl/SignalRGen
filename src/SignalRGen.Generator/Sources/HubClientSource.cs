using System.Text;
using Microsoft.CodeAnalysis.Text;

namespace SignalRGen.Generator.Sources;

internal static class HubClientSource
{
    private const string HubClientTemplate = """
                                             {~{autoGeneratedHint}~}

                                             #nullable enable

                                             namespace {~{namespaceName}~};

                                             /// <summary>
                                             /// Represents a HubClient for the <see cref = "{~{hubClientInterface}~}"/> interface.
                                             /// </summary>
                                             public class {~{hubName}~} : HubClientBase
                                             {
                                                 public static string HubUri { get; } = "{~{hubUri}~}";
                                                 public {~{hubName}~}(
                                                     global::System.Action<global::Microsoft.AspNetCore.SignalR.Client.IHubConnectionBuilder>? hubConnectionBuilderConfiguration,
                                                     global::System.Uri baseHubUri,
                                                     global::System.Action<global::Microsoft.AspNetCore.Http.Connections.Client.HttpConnectionOptions>? httpConnectionOptionsConfiguration)
                                                     : base(hubConnectionBuilderConfiguration, baseHubUri, httpConnectionOptionsConfiguration)
                                                 {
                                                 }
                                                 
                                             {~{serverToClientMethods}~}
                                             
                                             {~{clientToServerMethods}~}
                                             
                                                 
                                                 protected override void RegisterHubMethods()
                                                 {
                                                     if (_hubConnection is null)
                                                     {
                                                         return;
                                                     }
                                                 {~{onMethods}~}
                                                 }
                                                 
                                                 private void ValidateHubConnection()
                                                 {
                                                     if (_hubConnection is null)
                                                     {
                                                         throw new global::System.InvalidOperationException("The HubConnection is not started! Call `StartAsync` before initiating any actions.");
                                                     }
                                                 }
                                             }
                                             """;
    
    private const string FuncWithParams = "public global::System.Func<{~{parameterTypes}~}, global::System.Threading.Tasks.Task>? On{~{identifier}~} = default;";
    private const string FuncNoParams = "public global::System.Func<global::System.Threading.Tasks.Task>? On{~{identifier}~} = default;";
    
    private const string ServerToClientMethodTemplate = """
                                                            /// <summary>
                                                            /// Is invoked whenever the client method {~{identifier}~} of the <see cref = "{~{hubClientInterface}~}"/> gets invoked.
                                                            /// </summary>
                                                            {~{func}~}
                                                            private global::System.Threading.Tasks.Task {~{identifier}~}Handler({~{parameterList}~})
                                                            {
                                                                return On{~{identifier}~}?.Invoke({~{parameters}~}) ?? global::System.Threading.Tasks.Task.CompletedTask;
                                                            }
                                                        """;

    private const string ClientToServerMethodWithParamsTemplate =
        """
            /// <summary>
            /// Can be invoked to trigger the {~{identifier}~} on the <see cref = "{~{hubClientInterface}~}"/>.
            /// </summary>
            /// <exception cref="global::System.InvalidOperationException">Thrown, when the Hub was not yet started by calling <see cref="{~{hubName}~}.StartAsync"/></exception>
            public {~{returnType}~} Invoke{~{identifier}~}Async({~{parameterList}~}, global::System.Threading.CancellationToken ct = default)
            {
                ValidateHubConnection();
                return InvokeCoreAsync{~{genericReturnType}~}("{~{identifier}~}", new object?[] { {~{parameters}~} }, cancellationToken: ct);
            }
        """;
    
    private const string ClientToServerMethodNoParamsTemplate =
        """
            /// <summary>
            /// Can be invoked to trigger the {~{identifier}~} on the <see cref = "{~{hubClientInterface}~}"/>.
            /// </summary>
            /// <exception cref="global::System.InvalidOperationException">Thrown, when the Hub was not yet started by calling <see cref="{~{hubName}~}.StartAsync"/></exception>
            public {~{returnType}~} Invoke{~{identifier}~}Async(global::System.Threading.CancellationToken ct = default)
            {
                ValidateHubConnection();
                return InvokeCoreAsync{~{genericReturnType}~}("{~{identifier}~}", cancellationToken: ct);
            }
        """;
    
    private const string OnMethodWithParamsTemplate = """
                                                          global::Microsoft.AspNetCore.SignalR.Client.HubConnectionExtensions.On<{~{parameterTypes}~}>(_hubConnection, "{~{identifier}~}", {~{identifier}~}Handler);
                                                      """;
    private const string OnMethodNoParamsTemplate = """
                                                        global::Microsoft.AspNetCore.SignalR.Client.HubConnectionExtensions.On(_hubConnection, "{~{identifier}~}", {~{identifier}~}Handler);
                                                    """;


    internal static SourceText GetSourceText(HubClientToGenerate hubClientToGenerate)
    {
        var fullInterfaceName = $"global::{hubClientToGenerate.InterfaceNamespace}.{hubClientToGenerate.InterfaceName}";
        
        var serverToClientMethods = hubClientToGenerate.ServerToClientMethods.Select(method =>
            {
                var parameterTypes = string.Join(", ", method.Parameters.Select(p => p.Type));
                var parameterList = string.Join(", ", method.Parameters.Select(p => $"{p.Type} {p.Name}"));
                var parameters = string.Join(", ", method.Parameters.Select(p => p.Name));

                return ServerToClientMethodTemplate
                    .Replace("{~{func}~}", parameterTypes.Length > 0 ? FuncWithParams : FuncNoParams)
                    .Replace("{~{hubClientInterface}~}", fullInterfaceName)
                    .Replace("{~{identifier}~}", method.Identifier)
                    // These are ugly hacks right now -.-
                    .Replace("{~{parameterTypes}~}", parameterTypes.Replace("*", ""))
                    .Replace("{~{parameterList}~}", parameterList).Replace("*", "")
                    .Replace("{~{parameters}~}", parameters).Replace("*", "");
            })
            .ToArray();

        var clientToServerMethods = hubClientToGenerate.ClientToServerMethods.Select(method =>
        {
            var parameterTypes = string.Join(", ", method.Parameters.Select(p => p.Type));
            var parameterList = string.Join(", ", method.Parameters.Select(p => $"{p.Type} {p.Name}"));
            var parameters = string.Join(", ", method.Parameters.Select(p => p.Name));

            var templateToUse = parameterTypes.Length > 0
                ? ClientToServerMethodWithParamsTemplate
                : ClientToServerMethodNoParamsTemplate;
            
            var template = templateToUse
                .Replace("{~{hubClientInterface}~}", hubClientToGenerate.InterfaceName)
                .Replace("{~{hubName}~}", hubClientToGenerate.HubName)
                .Replace("{~{identifier}~}", method.Identifier)
                .Replace("{~{parameterTypes}~}", parameterTypes)
                .Replace("{~{parameterList}~}", parameterList)
                .Replace("{~{parameters}~}", parameters)
                // These two are ugly hacks right now -.-
                .Replace("{~{returnType}~}", method.ReturnType)
                .Replace("{~{genericReturnType}~}", method.AwaitableReturnType is not null ? $"<{method.AwaitableReturnType}>" : string.Empty);

            return template;
        });

        var onMethods = hubClientToGenerate.ServerToClientMethods
            .Select(method =>
            {
                var parameterTypes = string.Join(", ", method.Parameters.Select(p => p.Type));

                return parameterTypes.Length > 0
                    ? OnMethodWithParamsTemplate
                        .Replace("{~{identifier}~}", method.Identifier)
                        .Replace("{~{parameterTypes}~}", parameterTypes)
                        // Ugly hack right now -.-
                        .Replace("*", "")
                    : OnMethodNoParamsTemplate
                        .Replace("{~{identifier}~}", method.Identifier)
                        // Ugly hack right now -.-
                        .Replace("*", "");
            })
            .ToArray();

        var template = HubClientTemplate
            .Replace("{~{autoGeneratedHint}~}", AutoGeneratedHintSource.AutoGeneratedHintTemplate)
            // .Replace("{~{usings}~}", usings)
            .Replace("{~{namespaceName}~}", hubClientToGenerate.InterfaceNamespace)
            .Replace("{~{hubName}~}", hubClientToGenerate.HubName)
            .Replace("{~{hubUri}~}", hubClientToGenerate.HubUri)
            .Replace("{~{hubClientInterface}~}", hubClientToGenerate.InterfaceName)
            .Replace("{~{serverToClientMethods}~}", string.Join("\n", serverToClientMethods))
            .Replace("{~{clientToServerMethods}~}", string.Join("\n", clientToServerMethods))
            .Replace("{~{onMethods}~}", string.Join("\n\t", onMethods));


        return SourceText.From(template, Encoding.UTF8);
    }
}