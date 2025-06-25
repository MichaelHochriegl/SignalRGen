
namespace SignalRGen.Generator.Tests;

public class HubClientTests
{
    [Fact]
    public Task Generates_HubClient_Correctly()
    {
        // Arrange
        const string source = """
                              using SignalRGen.Generator;
                              using SignalRGen.Generator.Tests.TestData;
                              using SignalRGen.Abstractions.Attributes;
                              
                              namespace SignalRGen.Clients;
                              
                              [HubClient(HubName = "TestHubClient", HubUri = "examples")]
                              public interface ITestHubClient
                              {
                                Task ReceiveCustomTypeUpdate(IEnumerable<CustomTypeDto> customTypes);
                                Task ReceiveFooUpdate(string bar, int bass);
                                
                                [ServerToClientMethod]  
                                Task ReceiveNormalTypeWithSpecificAttributeApplied(string bazz, int buzz);
                                
                                [NoOpTest]
                                Task ReceiveWithArbitraryAttribute(int blub);
                                
                                [ClientToServerMethod]
                                Task SendClientToServerNoReturnType(string rick, int age);
                                
                                [ClientToServerMethod]
                                Task<string> SendClientToServerWithReturnType(string morty, bool partOfMission);
                              }
                              """;

        return TestHelper.Verify(source);
    }
    
    [Fact]
    public Task Generates_NamespaceNested_HubClient_Correctly()
    {
        // Arrange
        const string source = """
                              using SignalRGen.Generator;
                              using SignalRGen.Generator.Tests.TestData;
                              using SignalRGen.Abstractions.Attributes;

                              namespace SignalRGen.Clients {
                                    namespace Nested {
                                          [HubClient(HubName = "TestHubClient", HubUri = "examples")]
                                          public interface ITestHubClient
                                          {
                                            Task ReceiveCustomTypeUpdate(IEnumerable<CustomTypeDto> customTypes);
                                            Task ReceiveFooUpdate(string bar, int bass);
                                            
                                            [ServerToClientMethod]  
                                            Task ReceiveNormalTypeWithSpecificAttributeApplied(string bazz, int buzz);
                                            
                                            [NoOpTest]
                                            Task ReceiveWithArbitraryAttribute(int blub);
                                            
                                            [ClientToServerMethod]
                                            Task SendClientToServerNoReturnType(string rick, int age);
                                            
                                            [ClientToServerMethod]
                                            Task<string> SendClientToServerWithReturnType(string morty, bool partOfMission);
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
                              using SignalRGen.Generator;
                              using SignalRGen.Generator.Tests.TestData;
                              using SignalRGen.Abstractions.Attributes;

                              namespace SignalRGen.Clients;

                              [HubClient(HubName = "TestHubClient", HubUri = "examples")]
                              public interface ITestHubClient
                              {
                                Task NotifyNoAttributeApplied();
                                
                                [ServerToClientMethod]  
                                Task NotifyServerToClient();
                                
                                [ClientToServerMethod]
                                Task NotifyClientToServer();
                                
                                Task<string> NotifyWithReturnNoAttributeApplied();
                              
                                [ServerToClientMethod]  
                                Task<string> NotifyWithReturnServerToClient();
                                
                                [ClientToServerMethod]
                                Task<string> NotifyWithReturnClientToServer();
                              }
                              """;

        return TestHelper.Verify(source);
    }

    [Fact]
    public Task Generates_HubClient_WithComplexGenericTypes_Correctly()
    {
        // Arrange
        const string source = """
                              using SignalRGen.Generator;
                              using SignalRGen.Generator.Tests.TestData;
                              using SignalRGen.Abstractions.Attributes;

                              namespace SignalRGen.Clients;

                              [HubClient(HubName = "GenericTestHubClient", HubUri = "generics")]
                              public interface IGenericTestHubClient
                              {
                                Task ReceiveGenericList(List<CustomTypeDto> items);
                                Task ReceiveDictionary(Dictionary<string, int> keyValuePairs);
                                Task ReceiveNestedGeneric(Dictionary<string, List<CustomTypeDto>> complexData);
                                Task<Dictionary<int, string>> SendAndReceiveGeneric(List<CustomTypeDto> input);
                                
                                [ClientToServerMethod]
                                Task<Task<string>> SendNestedTask(Task<int> nestedTask);
                              }
                              """;

        return TestHelper.Verify(source);
    }

    [Fact]
    public Task Generates_HubClient_WithNullableTypes_Correctly()
    {
        // Arrange
        const string source = """
                              using SignalRGen.Generator;
                              using SignalRGen.Generator.Tests.TestData;
                              using SignalRGen.Abstractions.Attributes;

                              namespace SignalRGen.Clients;

                              [HubClient(HubName = "NullableTestHubClient", HubUri = "nullables")]
                              public interface INullableTestHubClient
                              {
                                Task ReceiveNullableString(string? nullableMessage);
                                Task ReceiveNullableInt(int? nullableNumber);
                                Task ReceiveNullableCustomType(CustomTypeDto? nullableDto);
                                
                                [ClientToServerMethod]
                                Task<string?> SendAndReceiveNullable(int? input);
                                
                                [ServerToClientMethod]
                                Task NotifyWithNullableArray(string[]? nullableArray);
                              }
                              """;

        return TestHelper.Verify(source);
    }

    [Fact]
    public Task Generates_HubClient_WithArrayTypes_Correctly()
    {
        // Arrange
        const string source = """
                              using SignalRGen.Generator;
                              using SignalRGen.Generator.Tests.TestData;
                              using SignalRGen.Abstractions.Attributes;

                              namespace SignalRGen.Clients;

                              [HubClient(HubName = "ArrayTestHubClient", HubUri = "arrays")]
                              public interface IArrayTestHubClient
                              {
                                Task ReceiveStringArray(string[] messages);
                                Task ReceiveIntArray(int[] numbers);
                                Task ReceiveCustomTypeArray(CustomTypeDto[] dtos);
                                Task ReceiveMultidimensionalArray(int[,] matrix);
                                Task ReceiveJaggedArray(string[][] jaggedArray);
                                
                                [ClientToServerMethod]
                                Task<string[]> SendAndReceiveArray(int[] input);
                              }
                              """;

        return TestHelper.Verify(source);
    }

    [Fact(Skip = "Related GitHub Issue: https://github.com/MichaelHochriegl/SignalRGen/issues/71")]
    public Task Generates_HubClient_WithDefaultValues_Correctly()
    {
        // Arrange
        const string source = """
                              using SignalRGen.Generator;
                              using SignalRGen.Generator.Tests.TestData;
                              using SignalRGen.Abstractions.Attributes;

                              namespace SignalRGen.Clients;

                              [HubClient(HubName = "DefaultValueTestHubClient", HubUri = "defaults")]
                              public interface IDefaultValueTestHubClient
                              {
                                Task ReceiveWithDefaults(string message, int count = 1, bool isActive = true);
                                Task ReceiveWithNullDefault(string message, string? optionalMessage = null);
                                
                                [ClientToServerMethod]
                                Task SendWithDefaults(string message, int priority = 0);
                                
                                [ClientToServerMethod]
                                Task<string> SendAndReceiveWithDefaults(string input, bool includeTimestamp = false);
                              }
                              """;

        return TestHelper.Verify(source);
    }

    [Fact]
    public Task Generates_HubClient_WithEmptyInterface_Correctly()
    {
        // Arrange
        const string source = """
                              using SignalRGen.Generator;
                              using SignalRGen.Abstractions.Attributes;

                              namespace SignalRGen.Clients;

                              [HubClient(HubName = "EmptyTestHubClient", HubUri = "empty")]
                              public interface IEmptyTestHubClient
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
                              using SignalRGen.Generator;
                              using SignalRGen.Abstractions.Attributes;

                              namespace SignalRGen.Clients;

                              [HubClient(HubName = "LongMethodNamesTestHubClient", HubUri = "longmethods")]
                              public interface ILongMethodNamesTestHubClient
                              {
                                Task ReceiveVeryLongMethodNameThatExceedsNormalExpectationsForMethodNaming(string message);
                                
                                [ClientToServerMethod]
                                Task SendAnotherVeryLongMethodNameWithMultipleParametersAndComplexSignature(string firstParam, int secondParam, bool thirdParam);
                                
                                [ClientToServerMethod]
                                Task<string> RequestVeryLongMethodNameWithReturnTypeAndMultipleComplexParameters(Dictionary<string, List<int>> complexParam);
                              }
                              """;

        return TestHelper.Verify(source);
    }

    [Fact]
    public Task Generates_HubClient_WithValueTupleParameters_Correctly()
    {
        // Arrange
        const string source = """
                              using SignalRGen.Generator;
                              using SignalRGen.Abstractions.Attributes;

                              namespace SignalRGen.Clients;

                              [HubClient(HubName = "ValueTupleTestHubClient", HubUri = "valuetuples")]
                              public interface IValueTupleTestHubClient
                              {
                                Task ReceiveTuple((string name, int age) person);
                                Task ReceiveNestedTuple((string, (int, bool)) complexTuple);
                                Task ReceiveNamedTuple((string FirstName, string LastName, int Age) person);
                                
                                [ClientToServerMethod]
                                Task<(bool success, string message)> SendAndReceiveTuple((int id, string data) input);
                              }
                              """;

        return TestHelper.Verify(source);
    }
}