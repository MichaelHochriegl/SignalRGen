using Bunit;
using Microsoft.Extensions.DependencyInjection;
using SignalRGen.Example.Client.BlazorServer.Pages;
using SignalRGen.Example.Contracts;
using SignalRGen.Example.Contracts.TestFakes;

namespace SignalRGen.Example.BlazorServer.Tests.Pages;

public class ChatTests : BunitContext
{
    // private readonly FakeChatHubContractClient _fakeClient;

    public ChatTests()
    {
        // _fakeClient = new FakeChatHubContractClient();
        
        Services.AddSingleton<ChatHubContractClient, FakeChatHubContractClient>();
    }

    [Fact]
    public void Join_Chat_Updates_UI_To_Chat_Mode()
    {
        // Arrange
        var cut = Render<Chat>();

        // Act
        cut.Find("input[placeholder='Enter username']").Change("TestUser");
        cut.Find("button").Click();

        // Assert
        cut.Find(".chatting-as-section span").MarkupMatches("<span>TestUser</span>");
        Assert.NotNull(cut.Find("input[placeholder='Enter message']"));
    }

    [Fact]
    public async Task Receiving_UserJoined_Event_Shows_Notification()
    {
        // Arrange
        var fakeClient = Services.GetRequiredService<ChatHubContractClient>() as FakeChatHubContractClient;
        var cut = Render<Chat>();
        cut.Find("input[placeholder='Enter username']").Change("Me");
        await cut.Find("button").ClickAsync();

        // Act
        await fakeClient.SimulateUserJoinedAsync("NewUser");

        // Assert
        await cut.WaitForStateAsync(() => 
            cut.FindAll(".messages div").Any(m => m.TextContent.Contains("User 'NewUser' joined")));
    }

    [Fact]
    public async Task Receiving_UserLeft_Event_Shows_Notification()
    {
        // Arrange
        
        var fakeClient = Services.GetRequiredService<ChatHubContractClient>() as FakeChatHubContractClient;
        var cut = Render<Chat>();
        cut.Find("input[placeholder='Enter username']").Change("Me");
        await cut.Find("button").ClickAsync();

        // Act
        await fakeClient.SimulateUserLeftAsync("OldUser");

        // Assert
        await cut.WaitForStateAsync(() => 
            cut.FindAll(".messages div").Any(m => m.TextContent.Contains("User 'OldUser' left")));
    }

    [Fact]
    public async Task Receiving_Message_Shows_In_Chat()
    {
        // Arrange
        
        var fakeClient = Services.GetRequiredService<ChatHubContractClient>() as FakeChatHubContractClient;
        var cut = Render<Chat>();
        cut.Find("input[placeholder='Enter username']").Change("Me");
        await cut.Find("button").ClickAsync();

        // Act
        await fakeClient.SimulateMessageReceivedAsync("Alice", "Hello World");

        // Assert
        await cut.WaitForStateAsync(() => 
            cut.FindAll(".messages div").Any(m => m.TextContent.Contains("Alice: Hello World")));
    }

    [Fact]
    public void Sending_Message_Calls_Hub_Method_And_Updates_UI()
    {
        // Arrange
        
        var fakeClient = Services.GetRequiredService<ChatHubContractClient>() as FakeChatHubContractClient;
        var cut = Render<Chat>();
        cut.Find("input[placeholder='Enter username']").Change("Me");
        cut.Find("button").Click();

        // Act
        cut.Find("input[placeholder='Enter message']").Change("My secret message");
        cut.Find("button").Click();

        // Assert
        Assert.Contains("My secret message", fakeClient.SendMessageCalls);
        
        cut.WaitForState(() => 
            cut.FindAll(".messages div").Any(m => m.TextContent.Contains("Me: My secret message")));
    }
}