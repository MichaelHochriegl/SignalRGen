using Bunit;
using Microsoft.Extensions.DependencyInjection;
using SignalRGen.Example.Client.BlazorServer.Pages;
using SignalRGen.Example.Contracts;
using SignalRGen.Example.Contracts.TestFakes;

namespace SignalRGen.Example.BlazorServer.Tests.Pages;

public class ChatTests : BunitContext, IAsyncLifetime
{
    public ChatTests()
    {
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
        ArgumentNullException.ThrowIfNull(fakeClient);
        var cut = Render<Chat>();
        cut.Find("input[placeholder='Enter username']").Change("Me");
        await cut.Find("button").ClickAsync();
    
        // Act
        await fakeClient.SimulateOnUserJoinedAsync("NewUser");
    
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
        await fakeClient.SimulateOnUserLeftAsync("OldUser");
    
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
        await fakeClient.SimulateOnMessageReceivedAsync("Alice", "Hello World");
    
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
    public Task InitializeAsync()
    {
        return Task.CompletedTask;
    }

    // This is bad currently, but to get it fixed, we need to alter the "real" generated HubClient
    // by implementing `Dispose`. Currently only `DisposeAsync` is implemented
    public new async Task DisposeAsync()
    {
        await base.DisposeAsync();
    }
}