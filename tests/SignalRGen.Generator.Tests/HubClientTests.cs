
namespace SignalRGen.Generator.Tests;

public class HubClientTests
{
    [Fact]
    public Task Generates_HubClient_Correctly()
    {
        // Arrange
        const string source = """
                              using System.Threading.Tasks;
                              using SignalRGen.Abstractions;
                              using SignalRGen.Abstractions.Attributes;
                              using SignalRGen.Generator.Tests.TestData;

                              namespace SignalRGen.Clients;

                              public interface ITestHubServerToClient
                              {
                                Task ReceiveCustomTypeUpdate(IEnumerable<CustomTypeDto> customTypes);
                                Task ReceiveFooUpdate(string bar, int bass);
                                Task ReceiveNormalTypeWithSpecificAttributeApplied(string bazz, int buzz);
                                Task ReceiveWithArbitraryAttribute(int blub);
                              }

                              public interface ITestHubClientToServer
                              {
                                Task SendClientToServerNoReturnType(string rick, int age);
                                Task<string> SendClientToServerWithReturnType(string morty, bool partOfMission);
                              }

                              [HubClient(HubName = "TestHubClient", HubUri = "examples")]
                              public interface ITestHub: IBidirectionalHub<ITestHubServerToClient, ITestHubClientToServer>
                              {
                              }
                              """;


        return TestHelper.Verify(source);
    }
    
    [Fact]
    public Task Generates_NamespaceNested_HubClient_Correctly()
    {
        // Arrange
        const string source = """
                              using System.Threading.Tasks;
                              using SignalRGen.Abstractions;
                              using SignalRGen.Abstractions.Attributes;
                              using SignalRGen.Generator.Tests.TestData;

                              namespace SignalRGen.Clients {
                                    namespace Nested {
                                          public interface ITestHubServerToClient
                                          {
                                            Task ReceiveCustomTypeUpdate(IEnumerable<CustomTypeDto> customTypes);
                                            Task ReceiveFooUpdate(string bar, int bass);
                                            Task ReceiveNormalTypeWithSpecificAttributeApplied(string bazz, int buzz);
                                            Task ReceiveWithArbitraryAttribute(int blub);
                                          }
                                          
                                          public interface ITestHubClientToServer
                                          {
                                            Task SendClientToServerNoReturnType(string rick, int age);
                                            Task<string> SendClientToServerWithReturnType(string morty, bool partOfMission);
                                          }
                                          
                                          [HubClient(HubName = "TestHubClient", HubUri = "examples")]
                                          public interface ITestHub : IBidirectionalHub<ITestHubServerToClient, ITestHubClientToServer>
                                          {
                                          }
                                    }
                              }
                              """;


        return TestHelper.Verify(source);
    }
    
    [Fact]
    public Task Generates_HubClient_WithNoPayload_Correctly()
    {
        // Arrange
        const string source = """
                              using System.Threading.Tasks;
                              using SignalRGen.Abstractions;
                              using SignalRGen.Abstractions.Attributes;

                              namespace SignalRGen.Clients;

                              public interface ITestHubServerToClient
                              {
                                Task NotifyNoAttributeApplied();
                                Task NotifyServerToClient();
                                Task<string> NotifyWithReturnNoAttributeApplied();
                                Task<string> NotifyWithReturnServerToClient();
                              }

                              public interface ITestHubClientToServer
                              {
                                Task NotifyClientToServer();
                                Task<string> NotifyWithReturnClientToServer();
                              }

                              [HubClient(HubName = "TestHubClient", HubUri = "examples")]
                              public interface ITestHub : IBidirectionalHub<ITestHubServerToClient, ITestHubClientToServer>
                              {
                              }
                              """;


        return TestHelper.Verify(source);
    }

    [Fact]
    public Task Generates_HubClient_WithComplexGenericTypes_Correctly()
    {
        // Arrange
        const string source = """
                              using System.Threading.Tasks;
                              using SignalRGen.Abstractions;
                              using SignalRGen.Abstractions.Attributes;
                              using SignalRGen.Generator.Tests.TestData;

                              namespace SignalRGen.Clients;

                              public interface IGenericTestHubServerToClient
                              {
                                Task ReceiveGenericList(List<CustomTypeDto> items);
                                Task ReceiveDictionary(Dictionary<string, int> keyValuePairs);
                                Task ReceiveNestedGeneric(Dictionary<string, List<CustomTypeDto>> complexData);
                                Task<Dictionary<int, string>> SendAndReceiveGeneric(List<CustomTypeDto> input);
                              }

                              public interface IGenericTestHubClientToServer
                              {
                                Task<Task<string>> SendNestedTask(Task<int> nestedTask);
                              }

                              [HubClient(HubName = "GenericTestHubClient", HubUri = "generics")]
                              public interface IGenericTestHub : IBidirectionalHub<IGenericTestHubServerToClient, IGenericTestHubClientToServer>
                              {
                              }
                              """;


        return TestHelper.Verify(source);
    }

    [Fact]
    public Task Generates_HubClient_WithNullableTypes_Correctly()
    {
        // Arrange
        const string source = """
                             using System.Threading.Tasks;
                             using SignalRGen.Abstractions;
                             using SignalRGen.Abstractions.Attributes;
                             using SignalRGen.Generator.Tests.TestData;

                             namespace SignalRGen.Clients;

                             public interface INullableTestHubServerToClient
                             {
                               Task ReceiveNullableString(string? nullableMessage);
                               Task ReceiveNullableInt(int? nullableNumber);
                               Task ReceiveNullableCustomType(CustomTypeDto? nullableDto);
                               Task NotifyWithNullableArray(string[]? nullableArray);
                             }

                             public interface INullableTestHubClientToServer
                             {
                               Task<string?> SendAndReceiveNullable(int? input);
                             }

                             [HubClient(HubName = "NullableTestHubClient", HubUri = "nullables")]
                             public interface INullableTestHub : IBidirectionalHub<INullableTestHubServerToClient, INullableTestHubClientToServer>
                             {
                             }
                             """;


        return TestHelper.Verify(source);
    }

    [Fact]
    public Task Generates_HubClient_WithArrayTypes_Correctly()
    {
        // Arrange
        const string source = """
                              using System.Threading.Tasks;
                              using SignalRGen.Abstractions;
                              using SignalRGen.Abstractions.Attributes;
                              using SignalRGen.Generator.Tests.TestData;

                              namespace SignalRGen.Clients;

                              public interface IArrayTestHubServerToClient
                              {
                                Task ReceiveStringArray(string[] messages);
                                Task ReceiveIntArray(int[] numbers);
                                Task ReceiveCustomTypeArray(CustomTypeDto[] dtos);
                                Task ReceiveMultidimensionalArray(int[,] matrix);
                                Task ReceiveJaggedArray(string[][] jaggedArray);
                              }

                              public interface IArrayTestHubClientToServer
                              {
                                Task<string[]> SendAndReceiveArray(int[] input);
                              }

                              [HubClient(HubName = "ArrayTestHubClient", HubUri = "arrays")]
                              public interface IArrayTestHub : IBidirectionalHub<IArrayTestHubServerToClient, IArrayTestHubClientToServer>
                              {
                              }
                              """;


        return TestHelper.Verify(source);
    }

    [Fact(Skip = "Related GitHub Issue: https://github.com/MichaelHochriegl/SignalRGen/issues/71")]
    public Task Generates_HubClient_WithDefaultValues_Correctly()
    {
        // Arrange
        const string source = """
                              using System.Threading.Tasks;
                              using SignalRGen.Abstractions;
                              using SignalRGen.Abstractions.Attributes;
                              using SignalRGen.Generator.Tests.TestData;

                              namespace SignalRGen.Clients;

                              public interface IDefaultValueTestHubServerToClient
                              {
                                Task ReceiveWithDefaults(string message, int count = 1, bool isActive = true);
                                Task ReceiveWithNullDefault(string message, string? optionalMessage = null);
                              }

                              public interface IDefaultValueTestHubClientToServer
                              {
                                Task SendWithDefaults(string message, int priority = 0);
                                Task<string> SendAndReceiveWithDefaults(string input, bool includeTimestamp = false);
                              }

                              [HubClient(HubName = "DefaultValueTestHubClient", HubUri = "defaults")]
                              public interface IDefaultValueTestHub : IBidirectionalHub<IDefaultValueTestHubServerToClient, IDefaultValueTestHubClientToServer>
                              {
                              }
                              """;


        return TestHelper.Verify(source);
    }

    [Fact]
    public Task Generates_HubClient_WithEmptyInterface_Correctly()
    {
        // Arrange
        const string source = """
                              using System.Threading.Tasks;
                              using SignalRGen.Abstractions;
                              using SignalRGen.Abstractions.Attributes;

                              namespace SignalRGen.Clients;

                              public interface IEmptyTestHubServerToClient
                              {
                              }

                              public interface IEmptyTestHubClientToServer
                              {
                              }

                              [HubClient(HubName = "EmptyTestHubClient", HubUri = "empty")]
                              public interface IEmptyTestHub : IBidirectionalHub<IEmptyTestHubServerToClient, IEmptyTestHubClientToServer>
                              {
                              }
                              """;


        return TestHelper.Verify(source);
    }
    

    [Fact]
    public Task Generates_HubClient_WithLongMethodNames_Correctly()
    {
        // Arrange
        const string source = """
                              using System.Threading.Tasks;
                              using SignalRGen.Abstractions;
                              using SignalRGen.Abstractions.Attributes;

                              namespace SignalRGen.Clients;

                              public interface ILongMethodNamesTestHubServerToClient
                              {
                                Task ReceiveVeryLongMethodNameThatExceedsNormalExpectationsForMethodNaming(string message);
                              }

                              public interface ILongMethodNamesTestHubClientToServer
                              {
                                Task SendAnotherVeryLongMethodNameWithMultipleParametersAndComplexSignature(string firstParam, int secondParam, bool thirdParam);
                                Task<string> RequestVeryLongMethodNameWithReturnTypeAndMultipleComplexParameters(Dictionary<string, List<int>> complexParam);
                              }

                              [HubClient(HubName = "LongMethodNamesTestHubClient", HubUri = "longmethods")]
                              public interface ILongMethodNamesTestHub : IBidirectionalHub<ILongMethodNamesTestHubServerToClient, ILongMethodNamesTestHubClientToServer>
                              {
                              }
                              """;


        return TestHelper.Verify(source);
    }

    [Fact]
    public Task Generates_HubClient_WithValueTupleParameters_Correctly()
    {
        // Arrange
        const string source = """
                              using System.Threading.Tasks;
                              using SignalRGen.Abstractions;
                              using SignalRGen.Abstractions.Attributes;

                              namespace SignalRGen.Clients;

                              public interface IValueTupleTestHubServerToClient
                              {
                                Task ReceiveTuple((string name, int age) person);
                                Task ReceiveNestedTuple((string, (int, bool)) complexTuple);
                                Task ReceiveNamedTuple((string FirstName, string LastName, int Age) person);
                              }

                              public interface IValueTupleTestHubClientToServer
                              {
                                Task<(bool success, string message)> SendAndReceiveTuple((int id, string data) input);
                              }

                              [HubClient(HubName = "ValueTupleTestHubClient", HubUri = "valuetuples")]
                              public interface IValueTupleTestHub : IBidirectionalHub<IValueTupleTestHubServerToClient, IValueTupleTestHubClientToServer>
                              {
                              }
                              """;


        return TestHelper.Verify(source);
    }
}