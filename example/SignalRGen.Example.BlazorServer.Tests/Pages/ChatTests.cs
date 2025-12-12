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
    public async Task Join_Chat_Updates_UI_To_Chat_Mode()
    {
        // Arrange
        await using var fakeClient = await GetFake();
        var cut = Render<Chat>();

        // Act
        cut.Find("#username-input").Input("TestUser");
        await cut.Find("#join-button").ClickAsync();

        // Assert
        var userName = cut.Find(".chat-header span strong");
        Assert.Equal("TestUser", userName.TextContent);
        Assert.NotNull(cut.Find("#message-input"));
    }

    [Fact]
    public async Task Receiving_UserJoined_Event_Shows_Notification()
    {
        // Arrange
        await using var fakeClient = await GetFake();
        var cut = Render<Chat>();
        cut.Find("#username-input").Input("Me");
        await cut.Find("#join-button").ClickAsync();
    
        // Act
        await fakeClient.SimulateOnUserJoinedAsync("NewUser");
    
        // Assert
        await fakeClient.WaitForOnUserJoinedAsync();
        await cut.WaitForStateAsync(() => 
            cut.FindAll(".msg").Any(m => m.TextContent.Contains("User 'NewUser' joined")));
    }

    

    [Fact]
    public async Task Receiving_UserLeft_Event_Shows_Notification()
    {
        // Arrange
        
        await using var fakeClient = await GetFake();
        var cut = Render<Chat>();
        cut.Find("#username-input").Input("Me");
        await cut.Find("#join-button").ClickAsync();
    
        // Act
        await fakeClient.SimulateOnUserLeftAsync("OldUser");
    
        // Assert
        await fakeClient.WaitForOnUserLeftAsync();
        await cut.WaitForStateAsync(() => 
            cut.FindAll(".msg").Any(m => m.TextContent.Contains("User 'OldUser' left")));
    }
    
    [Fact]
    public async Task Receiving_Message_Shows_In_Chat()
    {
        // Arrange
        
        await using var fakeClient = await GetFake();
        var cut = Render<Chat>();
        cut.Find("#username-input").Input("Me");
        await cut.Find("#join-button").ClickAsync();
    
        // Act
        await fakeClient.SimulateOnMessageReceivedAsync(new ChatMessage("Alice", "Hello World", DateTime.UtcNow));
    
        // Assert
        await fakeClient.WaitForOnMessageReceivedAsync();
        await cut.WaitForStateAsync(() =>
        {
            var readOnlyList = cut.FindAll(".msg");
            return readOnlyList.Any(m => m.TextContent.Contains("Alice: Hello World"));
        });
    }
    
    [Fact]
    public async Task Sending_Message_Calls_Hub_Method_And_Updates_UI()
    {
        // Arrange
        
        await using var fakeClient = await GetFake();
        var cut = Render<Chat>();
        cut.Find("#username-input").Input("Me");
        await cut.Find("#join-button").ClickAsync();
    
        // Act
        cut.Find("#message-input").Input("My secret message");
        await cut.Find("#send-button").ClickAsync();
    
        // Assert
        Assert.Contains("My secret message", fakeClient.SendMessageCalls);
        
        await cut.WaitForStateAsync(() => 
            cut.FindAll(".msg").Any(m => m.TextContent.Contains("Me: My secret message")));
    }

    private async Task<FakeChatHubContractClient> GetFake()
    {
        FakeChatHubContractClient? fakeClient = null;
        try
        {
            fakeClient = Services.GetRequiredService<ChatHubContractClient>() as FakeChatHubContractClient;
            ArgumentNullException.ThrowIfNull(fakeClient);
            return fakeClient;
        }
        catch
        {
            if (fakeClient != null) await fakeClient.DisposeAsync();
            throw;
        }
    }

    public Task InitializeAsync()
    {
        return Task.CompletedTask;
    }

    public new async Task DisposeAsync()
    {
        await base.DisposeAsync();
    }
}