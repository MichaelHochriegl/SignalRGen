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

        return TestHelper.Verify(source, true);
    }
}