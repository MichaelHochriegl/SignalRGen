namespace SignalRGen.Generator.Tests;

public class MultiProjectSupportTests
{
    [Fact]
    public Task Generates_WithCustom_SignalRModuleName_Correctly()
    {
        // Arrange
        const string source = """
                              using System.Threading.Tasks;
                              using SignalRGen.Abstractions;
                              using SignalRGen.Abstractions.Attributes;

                              namespace MyCompany.App.Clients;

                              public interface IPingServerToClient
                              {
                                  Task Ping();
                              }

                              public interface IPingClientToServer
                              {
                                  Task Pong();
                              }

                              [HubClient(HubName = "PingHubClient", HubUri = "ping")]
                              public interface IPingHub : IBidirectionalHub<IPingServerToClient, IPingClientToServer>
                              {
                              }
                              """;

        var analyzerOptions = new TestHelper.DictionaryAnalyzerConfigOptions(new Dictionary<string, string>
        {
            ["build_property.RootNamespace"] = "MyCompany.App",
            ["build_property.SignalRModuleName"] = "Payments"
        });

        // Act + Assert (snapshot)
        return TestHelper.Verify(source, analyzerOptions: analyzerOptions);
    }
    
    [Fact]
    public Task Generates_WithDefault_SignalRModuleName_WhenMissing()
    {
        // Arrange
        const string source = """
                              using System.Threading.Tasks;
                              using SignalRGen.Abstractions;
                              using SignalRGen.Abstractions.Attributes;

                              namespace MyCompany.App.Clients;

                              public interface IDefaultServerToClient
                              {
                                  Task Notify();
                              }

                              public interface IDefaultClientToServer
                              {
                                  Task Ack();
                              }

                              [HubClient(HubName = "DefaultHubClient", HubUri = "default")]
                              public interface IDefaultHub : IBidirectionalHub<IDefaultServerToClient, IDefaultClientToServer>
                              {
                              }
                              """;

        // Only provide RootNamespace; omit SignalRModuleName to assert default behavior
        var analyzerOptions = new TestHelper.DictionaryAnalyzerConfigOptions(new Dictionary<string, string>
        {
            ["build_property.RootNamespace"] = "MyCompany.App"
        });

        // Act + Assert (snapshot)
        return TestHelper.Verify(source, analyzerOptions: analyzerOptions);
    }
}