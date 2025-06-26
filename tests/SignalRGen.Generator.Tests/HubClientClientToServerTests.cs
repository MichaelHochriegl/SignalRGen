namespace SignalRGen.Generator.Tests;

public class HubClientClientToServerTests
{
    [Fact]
    public Task Generates_HubClient_WithClientToServerHub_Correctly()
    {
        // Tests the IClientToServerHub<TClient> marker interface for one-way client-to-server communication
        const string source = """
                              using System.Threading.Tasks;
                              using SignalRGen.Abstractions;
                              using SignalRGen.Abstractions.Attributes;
                              using SignalRGen.Generator.Tests.TestData;
                              
                              namespace SignalRGen.Clients;
                              
                              public interface ICommandClientToServer
                              {
                                Task SendCommand(string command, string payload);
                                Task<string> RequestData(int id);
                                Task<CustomTypeDto> GetCustomData(string filter);
                              }
                              
                              [HubClient(HubName = "CommandHubClient", HubUri = "commands")]
                              public interface ICommandHubClient : IClientToServerHub<ICommandClientToServer>
                              {
                              }
                              """;

        return TestHelper.Verify(source);
    }

    [Fact]
    public Task Generates_HubClient_WithClientToServerHub_WithComplexTypes_Correctly()
    {
        // Tests client-to-server hub with complex return types and parameters
        const string source = """
                              using System.Threading.Tasks;
                              using SignalRGen.Abstractions;
                              using SignalRGen.Abstractions.Attributes;
                              using SignalRGen.Generator.Tests.TestData;
                              
                              namespace SignalRGen.Clients;
                              
                              public interface IComplexCommandClientToServer
                              {
                                Task SendUserData(List<CustomTypeDto> users);
                                Task<Dictionary<string, int>> GetMetrics(string filter);
                                Task<string[]> RequestStringArray(int count);
                                Task<(bool success, string message)> ProcessTupleData((int id, string data) input);
                                Task SendNullableData(string? nullableMessage, int? nullableCount);
                              }
                              
                              [HubClient(HubName = "ComplexCommandHubClient", HubUri = "complex-commands")]
                              public interface IComplexCommandHubClient : IClientToServerHub<IComplexCommandClientToServer>
                              {
                              }
                              """;

        return TestHelper.Verify(source);
    }

    [Fact]
    public Task Generates_HubClient_WithClientToServerHub_WithInheritance_Correctly()
    {
        // Tests client-to-server hub with interface inheritance
        const string source = """
                              using System.Threading.Tasks;
                              using SignalRGen.Abstractions;
                              using SignalRGen.Abstractions.Attributes;
                              using SignalRGen.Generator.Tests.TestData;
                              
                              namespace SignalRGen.Clients;
                              
                              public interface IBaseCommandClientToServer
                              {
                                Task<string> GetBaseData(int id);
                              }
                              
                              public interface IExtendedCommandClientToServer : IBaseCommandClientToServer
                              {
                                Task SendExtendedCommand(string command, string payload);
                                Task<CustomTypeDto> GetExtendedData(string filter);
                              }
                              
                              [HubClient(HubName = "ExtendedCommandHubClient", HubUri = "extended-commands")]
                              public interface IExtendedCommandHubClient : IClientToServerHub<IExtendedCommandClientToServer>
                              {
                              }
                              """;

        return TestHelper.Verify(source);
    }

    [Fact]
    public Task Generates_HubClient_WithClientToServerHub_WithNoMethods_Correctly()
    {
        // Tests client-to-server hub with an empty interface
        const string source = """
                              using System.Threading.Tasks;
                              using SignalRGen.Abstractions;
                              using SignalRGen.Abstractions.Attributes;
                              
                              namespace SignalRGen.Clients;
                              
                              public interface IEmptyCommandClientToServer
                              {
                              }
                              
                              [HubClient(HubName = "EmptyCommandHubClient", HubUri = "empty-commands")]
                              public interface IEmptyCommandHubClient : IClientToServerHub<IEmptyCommandClientToServer>
                              {
                              }
                              """;

        return TestHelper.Verify(source);
    }

    [Fact]
    public Task Generates_HubClient_WithClientToServerHub_WithNestedNamespace_Correctly()
    {
        // Tests client-to-server hub with nested namespace
        const string source = """
                              using System.Threading.Tasks;
                              using SignalRGen.Abstractions;
                              using SignalRGen.Abstractions.Attributes;
                              using SignalRGen.Generator.Tests.TestData;

                              namespace SignalRGen.Clients {
                                    namespace Nested {
                                          public interface INestedCommandClientToServer
                                          {
                                            Task SendNestedCommand(string command);
                                            Task<CustomTypeDto> GetNestedData(int id);
                                          }
                                          
                                          [HubClient(HubName = "NestedCommandHubClient", HubUri = "nested-commands")]
                                          public interface INestedCommandHubClient : IClientToServerHub<INestedCommandClientToServer>
                                          {
                                          }
                                    }
                              }
                              """;

        return TestHelper.Verify(source);
    }

    [Fact]
    public Task Generates_HubClient_WithClientToServerHub_WithMultiLevelInheritance_Correctly()
    {
        // Tests client-to-server hub with multiple levels of inheritance
        const string source = """
                              using System.Threading.Tasks;
                              using SignalRGen.Abstractions;
                              using SignalRGen.Abstractions.Attributes;
                              using SignalRGen.Generator.Tests.TestData;
                              
                              namespace SignalRGen.Clients;
                              
                              public interface IBaseCommandClientToServer
                              {
                                Task<string> GetBaseData(int id);
                              }
                              
                              public interface IMidCommandClientToServer : IBaseCommandClientToServer
                              {
                                Task SendMidCommand(string command);
                              }
                              
                              public interface ITopCommandClientToServer : IMidCommandClientToServer
                              {
                                Task<CustomTypeDto> GetTopData(string filter);
                              }
                              
                              [HubClient(HubName = "MultiLevelCommandHubClient", HubUri = "multi-level-commands")]
                              public interface IMultiLevelCommandHubClient : IClientToServerHub<ITopCommandClientToServer>
                              {
                              }
                              """;

        return TestHelper.Verify(source);
    }

    [Fact]
    public Task Generates_HubClient_WithClientToServerHub_WithVoidMethods_Correctly()
    {
        // Tests client-to-server hub with methods that don't return values
        const string source = """
                              using System.Threading.Tasks;
                              using SignalRGen.Abstractions;
                              using SignalRGen.Abstractions.Attributes;
                              using SignalRGen.Generator.Tests.TestData;
                              
                              namespace SignalRGen.Clients;
                              
                              public interface IVoidCommandClientToServer
                              {
                                Task SendNotification(string message);
                                Task UpdateStatus(bool isOnline);
                                Task LogActivity(string activity, CustomTypeDto context);
                                Task ClearCache();
                              }
                              
                              [HubClient(HubName = "VoidCommandHubClient", HubUri = "void-commands")]
                              public interface IVoidCommandHubClient : IClientToServerHub<IVoidCommandClientToServer>
                              {
                              }
                              """;

        return TestHelper.Verify(source);
    }

    [Fact]
    public Task Generates_HubClient_WithClientToServerHub_WithNoParameters_Correctly()
    {
        // Tests client-to-server hub with parameterless methods
        const string source = """
                              using System.Threading.Tasks;
                              using SignalRGen.Abstractions;
                              using SignalRGen.Abstractions.Attributes;
                              using SignalRGen.Generator.Tests.TestData;
                              
                              namespace SignalRGen.Clients;
                              
                              public interface IParameterlessCommandClientToServer
                              {
                                Task Ping();
                                Task Disconnect();
                                Task<string> GetServerTime();
                                Task<CustomTypeDto> GetServerStatus();
                                Task<bool> IsAlive();
                              }
                              
                              [HubClient(HubName = "ParameterlessCommandHubClient", HubUri = "parameterless-commands")]
                              public interface IParameterlessCommandHubClient : IClientToServerHub<IParameterlessCommandClientToServer>
                              {
                              }
                              """;

        return TestHelper.Verify(source);
    }
}