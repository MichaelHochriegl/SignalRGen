using System.Text;
using Microsoft.CodeAnalysis.Text;
using SignalRGen.Generator.Common;

namespace SignalRGen.Generator.Sources;

internal static class HubClientSource
{
    private const string HubClientTemplate = """
                                             {~{autoGeneratedHint}~}

                                             {~{usings}~}

                                             #nullable enable

                                             namespace {~{namespaceName}~};

                                             /// <summary>
                                             /// Represents a HubClient for the <see cref = "{~{hubClientInterface}~}"/> interface.
                                             /// </summary>
                                             public class {~{hubName}~} : HubClientBase
                                             {
                                                 public static string HubUri { get; } = "{~{hubUri}~}";
                                                 public {~{hubName}~}(Action<IHubConnectionBuilder>? hubConnectionBuilderConfiguration, Uri baseHubUri, Action<HttpConnectionOptions>? httpConnectionOptionsConfiguration) : base(hubConnectionBuilderConfiguration, baseHubUri, httpConnectionOptionsConfiguration)
                                                 {
                                                 }
                                                 
                                             {~{serverToClientMethods}~}
                                             
                                             {~{clientToServerMethods}~}
                                             
                                                 
                                                 protected override void RegisterHubMethods()
                                                 {
                                                 {~{onMethods}~}
                                                 }
                                                 
                                                 private void ValidateHubConnection()
                                                 {
                                                     if (_hubConnection is null)
                                                     {
                                                         throw new InvalidOperationException("The HubConnection is not started! Call `StartAsync` before initiating any actions.");
                                                     }
                                                 }
                                             }
                                             """;
    
    private const string FuncWithParams = "public Func<{~{parameterTypes}~}, Task>? On{~{identifier}~} = default;";
    private const string FuncNoParams = "public Func<Task>? On{~{identifier}~} = default;";
    
    private const string ServerToClientMethodTemplate = """
                                                            /// <summary>
                                                            /// Is invoked whenever the client method {~{identifier}~} of the <see cref = "{~{hubClientInterface}~}"/> gets invoked.
                                                            /// </summary>
                                                            {~{func}~}
                                                            private Task {~{identifier}~}Handler({~{parameterList}~})
                                                            {
                                                                return On{~{identifier}~}?.Invoke({~{parameters}~}) ?? Task.CompletedTask;
                                                            }
                                                        """;

    private const string ClientToServerMethodWithParamsTemplate =
        """
            /// <summary>
            /// Can be invoked to trigger the {~{identifier}~} on the <see cref = "{~{hubClientInterface}~}"/>.
            /// </summary>
            /// <exception cref="InvalidOperationException">Thrown, when the Hub was not yet started by calling <see cref="{~{hubName}~}.StartAsync"/></exception>
            public {~{returnType}~} Invoke{~{identifier}~}Async({~{parameterList}~}, CancellationToken ct = default)
            {
                ValidateHubConnection();
                return _hubConnection!.InvokeAsync{~{genericReturnType}~}("{~{identifier}~}", {~{parameters}~}, cancellationToken: ct);
            }
        """;
    
    private const string ClientToServerMethodNoParamsTemplate =
        """
            /// <summary>
            /// Can be invoked to trigger the {~{identifier}~} on the <see cref = "{~{hubClientInterface}~}"/>.
            /// </summary>
            /// <exception cref="InvalidOperationException">Thrown, when the Hub was not yet started by calling <see cref="{~{hubName}~}.StartAsync"/></exception>
            public {~{returnType}~} Invoke{~{identifier}~}Async(CancellationToken ct = default)
            {
                ValidateHubConnection();
                return _hubConnection!.InvokeAsync{~{genericReturnType}~}("{~{identifier}~}", cancellationToken: ct);
            }
        """;
    
    private const string OnMethodWithParamsTemplate = """
                                                          _hubConnection?.On<{~{parameterTypes}~}>("{~{identifier}~}", {~{identifier}~}Handler);
                                                      """;
    private const string OnMethodNoParamsTemplate = """
                                                        _hubConnection?.On("{~{identifier}~}", {~{identifier}~}Handler);
                                                    """;

    internal static SourceText GetSourceText(HubClientToGenerate hubClientToGenerate)
    {
        var allUsings =
            hubClientToGenerate.Usings
                .Append(new CacheableUsingDeclaration("using Microsoft.AspNetCore.SignalR.Client;"))
                .Append(new CacheableUsingDeclaration("using Microsoft.AspNetCore.Http.Connections.Client;"));
        var usings = string.Join("\n", allUsings.Select(u => u.UsingNamespace));

        var serverToClientMethods = hubClientToGenerate.ServerToClientMethods.Select(method =>
            {
                var parameterTypes = string.Join(", ", method.Parameters.Select(p => p.Type));
                var parameterList = string.Join(", ", method.Parameters.Select(p => $"{p.Type} {p.Name}"));
                var parameters = string.Join(", ", method.Parameters.Select(p => p.Name));

                return ServerToClientMethodTemplate
                    .Replace("{~{func}~}", parameterTypes.Length > 0 ? FuncWithParams : FuncNoParams)
                    .Replace("{~{hubClientInterface}~}", hubClientToGenerate.InterfaceName)
                    .Replace("{~{identifier}~}", method.Identifier)
                    .Replace("{~{parameterTypes}~}", parameterTypes)
                    .Replace("{~{parameterList}~}", parameterList)
                    .Replace("{~{parameters}~}", parameters);
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
                .Replace("{~{returnType}~}", method.ReturnType.Replace("System.Threading.Tasks.", ""))
                .Replace("{~{genericReturnType}~}", method.ReturnType.Replace("System.Threading.Tasks.Task", ""));

            return template;
        });

        var onMethods = hubClientToGenerate.ServerToClientMethods
            .Select(method =>
            {
                var parameterTypes = string.Join(", ", method.Parameters.Select(p => p.Type));

                return parameterTypes.Length > 0
                    ? OnMethodWithParamsTemplate.Replace("{~{identifier}~}", method.Identifier)
                        .Replace("{~{parameterTypes}~}", parameterTypes)
                    : OnMethodNoParamsTemplate.Replace("{~{identifier}~}", method.Identifier);
            })
            .ToArray();

        var template = HubClientTemplate
            .Replace("{~{autoGeneratedHint}~}", AutoGeneratedHintSource.AutoGeneratedHintTemplate)
            .Replace("{~{usings}~}", usings)
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