
namespace SignalRGen.Generator.Tests;

public class HubClientServerToClientTests
{
    [Fact]
    public Task Generates_HubClient_WithServerToClientHub_Correctly()
    {
        // Tests the IServerToClientHub<TServer> marker interface for one-way server-to-client communication
        const string source = """
                              using System.Threading.Tasks;
                              using SignalRGen.Abstractions;
                              using SignalRGen.Abstractions.Attributes;
                              using SignalRGen.Generator.Tests.TestData;
                              
                              namespace SignalRGen.Clients;
                              
                              public interface INotificationServerToClient
                              {
                                Task ReceiveNotification(string message, string type);
                                Task ReceiveAlert(string alertMessage, int severity);
                                Task ReceiveCustomTypeUpdate(CustomTypeDto dto);
                              }
                              
                              [HubClient(HubName = "NotificationHubClient", HubUri = "notifications")]
                              public interface INotificationHubClient : IServerToClientHub<INotificationServerToClient>
                              {
                              }
                              """;

        return TestHelper.Verify(source);
    }

    [Fact]
    public Task Generates_HubClient_WithServerToClientHub_WithComplexTypes_Correctly()
    {
        // Tests server-to-client hub with complex generic types
        const string source = """
                              using System.Threading.Tasks;
                              using SignalRGen.Abstractions;
                              using SignalRGen.Abstractions.Attributes;
                              using SignalRGen.Generator.Tests.TestData;
                              
                              namespace SignalRGen.Clients;
                              
                              public interface IComplexNotificationServerToClient
                              {
                                Task ReceiveUserList(List<CustomTypeDto> users);
                                Task ReceiveDataUpdate(Dictionary<string, int> metrics);
                                Task ReceiveNullableData(string? nullableMessage, int? nullableCount);
                                Task ReceiveArrayData(string[] messages, CustomTypeDto[] dtos);
                                Task ReceiveTupleData((string name, int value) tupleData);
                              }
                              
                              [HubClient(HubName = "ComplexNotificationHubClient", HubUri = "complex-notifications")]
                              public interface IComplexNotificationHubClient : IServerToClientHub<IComplexNotificationServerToClient>
                              {
                              }
                              """;

        return TestHelper.Verify(source);
    }

    [Fact]
    public Task Generates_HubClient_WithServerToClientHub_WithInheritance_Correctly()
    {
        // Tests server-to-client hub with interface inheritance
        const string source = """
                              using System.Threading.Tasks;
                              using SignalRGen.Abstractions;
                              using SignalRGen.Abstractions.Attributes;
                              using SignalRGen.Generator.Tests.TestData;
                              
                              namespace SignalRGen.Clients;
                              
                              public interface IBaseNotificationServerToClient
                              {
                                Task ReceiveBaseNotification(string message);
                              }
                              
                              public interface IExtendedNotificationServerToClient : IBaseNotificationServerToClient
                              {
                                Task ReceiveExtendedNotification(string message, int priority);
                                Task ReceiveCustomTypeNotification(CustomTypeDto dto);
                              }
                              
                              [HubClient(HubName = "ExtendedNotificationHubClient", HubUri = "extended-notifications")]
                              public interface IExtendedNotificationHubClient : IServerToClientHub<IExtendedNotificationServerToClient>
                              {
                              }
                              """;

        return TestHelper.Verify(source);
    }

    [Fact]
    public Task Generates_HubClient_WithServerToClientHub_WithNoMethods_Correctly()
    {
        // Tests server-to-client hub with an empty interface
        const string source = """
                              using System.Threading.Tasks;
                              using SignalRGen.Abstractions;
                              using SignalRGen.Abstractions.Attributes;
                              
                              namespace SignalRGen.Clients;
                              
                              public interface IEmptyNotificationServerToClient
                              {
                              }
                              
                              [HubClient(HubName = "EmptyNotificationHubClient", HubUri = "empty-notifications")]
                              public interface IEmptyNotificationHubClient : IServerToClientHub<IEmptyNotificationServerToClient>
                              {
                              }
                              """;

        return TestHelper.Verify(source);
    }

    [Fact]
    public Task Generates_HubClient_WithServerToClientHub_WithNestedNamespace_Correctly()
    {
        // Tests server-to-client hub with nested namespace
        const string source = """
                              using System.Threading.Tasks;
                              using SignalRGen.Abstractions;
                              using SignalRGen.Abstractions.Attributes;
                              using SignalRGen.Generator.Tests.TestData;

                              namespace SignalRGen.Clients {
                                    namespace Nested {
                                          public interface INestedNotificationServerToClient
                                          {
                                            Task ReceiveNestedNotification(string message);
                                            Task ReceiveNestedData(CustomTypeDto dto);
                                          }
                                          
                                          [HubClient(HubName = "NestedNotificationHubClient", HubUri = "nested-notifications")]
                                          public interface INestedNotificationHubClient : IServerToClientHub<INestedNotificationServerToClient>
                                          {
                                          }
                                    }
                              }
                              """;

        return TestHelper.Verify(source);
    }

    [Fact]
    public Task Generates_HubClient_WithServerToClientHub_WithMultiLevelInheritance_Correctly()
    {
        // Tests server-to-client hub with multiple levels of inheritance
        const string source = """
                              using System.Threading.Tasks;
                              using SignalRGen.Abstractions;
                              using SignalRGen.Abstractions.Attributes;
                              using SignalRGen.Generator.Tests.TestData;
                              
                              namespace SignalRGen.Clients;
                              
                              public interface IBaseNotificationServerToClient
                              {
                                Task ReceiveBaseNotification(string message);
                              }
                              
                              public interface IMidNotificationServerToClient : IBaseNotificationServerToClient
                              {
                                Task ReceiveMidNotification(int value);
                              }
                              
                              public interface ITopNotificationServerToClient : IMidNotificationServerToClient
                              {
                                Task ReceiveTopNotification(CustomTypeDto dto);
                              }
                              
                              [HubClient(HubName = "MultiLevelNotificationHubClient", HubUri = "multi-level-notifications")]
                              public interface IMultiLevelNotificationHubClient : IServerToClientHub<ITopNotificationServerToClient>
                              {
                              }
                              """;

        return TestHelper.Verify(source);
    }

    [Fact]
    public Task Generates_HubClient_WithServerToClientHub_WithNoParameters_Correctly()
    {
        // Tests server-to-client hub with parameterless methods
        const string source = """
                              using System.Threading.Tasks;
                              using SignalRGen.Abstractions;
                              using SignalRGen.Abstractions.Attributes;
                              
                              namespace SignalRGen.Clients;
                              
                              public interface IParameterlessNotificationServerToClient
                              {
                                Task ReceiveHeartbeat();
                                Task ReceiveRefreshSignal();
                              }
                              
                              [HubClient(HubName = "ParameterlessNotificationHubClient", HubUri = "parameterless-notifications")]
                              public interface IParameterlessNotificationHubClient : IServerToClientHub<IParameterlessNotificationServerToClient>
                              {
                              }
                              """;

        return TestHelper.Verify(source);
    }
}