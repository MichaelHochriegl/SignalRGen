using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using SignalRGen.Generator.Common;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace SignalRGen.Generator.Sources;

internal static class SignalRClientServiceRegistrationSource
{
    private const string SignalRClientServiceRegistrationTemplate =
        """
        {~{autoGeneratedHint}~}
        
        {~{usings}~}
        
        #nullable enable
        
        namespace SignalRGen.Generator.Client.Extensions.DependencyInjection;
        
        public static class SignalRClientServiceRegistration
        {
            /// <summary>
            /// Creates the base configuration for all registered Hubs. The Hubs you want to use must be registered with the appropriate `With{...}` call.
            /// </summary>
            /// <param name = "services">The services available in the application.</param>
            /// <param name = "generalConfiguration">An action used to configure the provided options.</param>
            /// <returns>The <see cref = "SignalRHubServiceCollection"/> to register the specified Hub.</returns>
            public static SignalRHubServiceCollection AddSignalRHubs(this IServiceCollection services, Action<SignalROptions> generalConfiguration)
            {
                ArgumentNullException.ThrowIfNull(generalConfiguration);
                var config = new SignalROptions();
                generalConfiguration.Invoke(config);
                return new SignalRHubServiceCollection(services, config);
            }
        
        {~{withHubMethods}~}
        
            private static IEnumerable<TimeSpan> DefaultRetrySteps
            {
                get
                {
                    var retrySteps = Enumerable.Repeat(TimeSpan.FromSeconds(1), 10);
                    retrySteps = retrySteps.Concat(Enumerable.Repeat(TimeSpan.FromSeconds(3), 5));
                    retrySteps = retrySteps.Concat(Enumerable.Repeat(TimeSpan.FromSeconds(10), 2));
                    return retrySteps;
                }
            }
        }
        """;
    
    private const string WithHubTemplate =
        """
            /// <summary>
            /// Registers the <see cref = "{~{hubName}~}"/> in the <see cref = "ServiceCollection"/>.
            /// </summary>
            /// <remarks>
            /// <para>
            /// By default the <see cref = "{~{hubName}~}"/> is registered with <see cref = "ServiceLifetime"/> singleton.
            /// </para>
            /// If no <see cref = "IRetryPolicy"/> is configured for the <see cref = "HubConnectionBuilder"/> a default retry policy will be used.
            /// <list type="bullet">
            ///     <item>
            ///         Every second - 10 attempts
            ///     </item>
            ///     <item>
            ///         Every 3 seconds - 5 attempts
            ///     </item>
            ///     <item>
            ///         Every 10 seconds - 2 attempts
            ///     </item>
            /// </list>
            /// </remarks>
            /// <param name = "services">The <see cref = "SignalRHubServiceCollection"/> to register the Hub.</param>
            /// <param name = "configuration">An action used to configure the provided options.</param>
            /// <returns>The <see cref = "SignalRHubServiceCollection"/> to register additional Hubs.</returns>
            public static SignalRHubServiceCollection With{~{hubName}~}(this SignalRHubServiceCollection services, Action<HubClientOptions>? configuration = null)
            {
                ArgumentNullException.ThrowIfNull(services);
                var config = new HubClientOptions();
                configuration?.Invoke(config);
                services.Services.Add(new ServiceDescriptor(typeof({~{hubName}~}), factory: _ =>
                {
                    var hubConnectionBuilder = new HubConnectionBuilder().WithUrl(new Uri(services.GeneralConfiguration.HubBaseUri, {~{hubName}~}.HubUri)).WithAutomaticReconnect(DefaultRetrySteps.ToArray());
                    config.HubConnectionBuilderConfiguration?.Invoke(hubConnectionBuilder);
                    return new {~{hubName}~}(hubConnectionBuilder);
                }, config.HubClientLifetime));
                return services;
            }
        """;
    
    internal static SourceText GetSource(EquatableArray<HubClientToGenerate> hubs)
    {
        var allUsings =
            hubs
                .Select(h => $"using {h.InterfaceNamespace};")
                .Append("using Microsoft.AspNetCore.SignalR.Client;")
                .Append("using Microsoft.Extensions.DependencyInjection;")
                .Append("using SignalRGen.Generator.Client.Configuration;")
                .Distinct();
        var usings = string.Join("\n", allUsings);

        var withHubMethods = hubs.Select(hub =>
        {
            var template = WithHubTemplate.Replace("{~{hubName}~}", hub.HubName);

            return template;
        });
        
        var template = SignalRClientServiceRegistrationTemplate
            .Replace("{~{autoGeneratedHint}~}", AutoGeneratedHintSource.AutoGeneratedHintTemplate)
            .Replace("{~{usings}~}", usings)
            .Replace("{~{withHubMethods}~}", string.Join("\n", withHubMethods));

        return SourceText.From(template, Encoding.UTF8);
    }
}
