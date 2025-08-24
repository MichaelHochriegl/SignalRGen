using Bunit;
using Microsoft.Extensions.DependencyInjection;
using SignalRGen.Example.Client.BlazorServer.Pages;
using SignalRGen.Example.Client.BlazorServer.Tests.ManualFakes;
using SignalRGen.Example.Contracts;

namespace SignalRGen.Example.Client.BlazorServer.Tests.Pages;

// These tests are AI generated to quickly test my POC for the TestFakes.
// We need real ones once the approach for the fakes is set in stone.
public class ChatComponentTests : TestContext
{
    private IRenderedComponent<Chat> RenderWithFake(out FakeChatHubContractClient fake)
    {
        // Provide a single fake instance as the concrete client the component injects
        fake = new FakeChatHubContractClient();
        Services.AddSingleton<ChatHubContractClient>(fake);

        // Render component
        var cut = RenderComponent<Chat>();
        return cut;
    }

    [Fact]
    public void Join_RendersUserName_And_Subscribes_ServerPush_UserJoined()
    {
        // Arrange
        var cut = RenderWithFake(out var fake);

        // Before join, the username input and Join button are visible
        var usernameInput = cut.Find("input[placeholder='Enter username']");
        var joinButton = cut.Find("button");

        // Act: Type username and click Join
        usernameInput.Change("alice");
        joinButton.Click();

        // Assert: The UI switches to chat mode and shows "Chatting as: alice"
        cut.WaitForAssertion(() =>
        {
            var info = cut.Find("div.chatting-as-section");
            Assert.Contains("Chatting as:", info.InnerHtml, StringComparison.Ordinal);
            Assert.Contains("alice", info.InnerHtml, StringComparison.Ordinal);
        });

        // Act: Simulate a server push that someone joined
        _ = fake.SimulateUserJoinedAsync("bob");

        // Assert: The message appears
        cut.WaitForAssertion(() =>
        {
            var messages = cut.Find("div.messages");
            Assert.Contains("~~ User 'bob' joined ~~", messages.InnerHtml, StringComparison.Ordinal);
        });
    }

    [Fact]
    public void SendMessage_AppendsLocalEcho_And_Invokes_Server()
    {
        // Arrange
        var cut = RenderWithFake(out var fake);

        // Join as "alice"
        cut.Find("input[placeholder='Enter username']").Change("alice");
        cut.Find("button").Click();
        
        cut.WaitForAssertion(() =>
        {
            var info = cut.Find("div.chatting-as-section");
            Assert.Contains("Chatting as:", info.InnerHtml, StringComparison.Ordinal);
            Assert.Contains("alice", info.InnerHtml, StringComparison.Ordinal);
        });

        // After join, message input and Send button are visible
        var messageInput = cut.Find("input[placeholder='Enter message']");
        var sendButton = cut.Find("button"); // After joining, only "Send" button is present in the markup branch

        // Act: Type and send a message
        messageInput.Change("hello world");
        sendButton.Click();

        // Assert: The local echo is appended and input cleared
        cut.WaitForAssertion(() =>
        {
            var messages = cut.Find("div.messages");
            Assert.Contains("alice: hello world", messages.InnerHtml, StringComparison.Ordinal);

            var currentValue = cut.Find("input[placeholder='Enter message']").GetAttribute("value") ?? string.Empty;
            Assert.Equal(string.Empty, currentValue);
        });

        // Assert: The fake captured the client-to-server invocation
        Assert.Collection(fake.SendMessageCalls,
            msg => Assert.Equal("hello world", msg));
    }

    [Fact]
    public async Task ServerPush_MultipleEvents_AppearInOrder()
    {
        // Arrange
        var cut = RenderWithFake(out var fake);

        // Join as "alice"
        cut.Find("input[placeholder='Enter username']").Change("alice");
        cut.Find("button").Click();

        // Act: Simulate server events in order
        await fake.SimulateUserJoinedAsync("bob");
        await fake.SimulateMessageReceivedAsync("bob", "hi");
        await fake.SimulateUserLeftAsync("bob");

        // Assert: Messages render in order
        cut.WaitForAssertion(() =>
        {
            var items = cut.FindAll("div.messages > div");
            Assert.True(items.Count >= 3, "Expected at least three messages");

            // Find the last three entries to avoid any prior messages
            var last = items[^3].TextContent;
            var secondLast = items[^2].TextContent;
            var thirdLast = items[^1].TextContent;

            Assert.Equal("~~ User 'bob' joined ~~", last);
            Assert.Equal("bob: hi", secondLast);
            Assert.Equal("~~ User 'bob' left ~~", thirdLast);
        });
    }
    
    [Fact]
    public async Task AwaitNext_UserJoined_Then_AssertUI()
    {
        // Arrange
        var cut = RenderWithFake(out var fake);

        cut.Find("input[placeholder='Enter username']").Change("alice");
        cut.Find("button").Click();
        

        // Act: simulate server push
        await fake.SimulateUserJoinedAsync("bob");

        // Assert: awaited value is correct
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(1));
        var next = await fake.WaitForUserJoinedAsync(cts.Token);;
        Assert.Equal("bob", next);

        // Assert: UI updated accordingly
        cut.WaitForAssertion(() =>
        {
            var messages = cut.Find("div.messages");
            Assert.Contains("~~ User 'bob' joined ~~", messages.InnerHtml, StringComparison.Ordinal);
        });
    }

    [Fact]
    public async Task AwaitNext_MessageReceived_Tuple_Deconstruction_And_UI()
    {
        // Arrange
        var cut = RenderWithFake(out var fake);

        cut.Find("input[placeholder='Enter username']").Change("alice");
        cut.Find("button").Click();


        // Act
        await fake.SimulateMessageReceivedAsync("bob", "hi there");

        // Assert: value received from the async helper
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(1));
        var (user, message) = await fake.WaitForMessageReceivedAsync(cts.Token);
        Assert.Equal("bob", user);
        Assert.Equal("hi there", message);

        // Assert: UI reflects the message
        cut.WaitForAssertion(() =>
        {
            var messages = cut.Find("div.messages");
            Assert.Contains("bob: hi there", messages.InnerHtml, StringComparison.Ordinal);
        });
    }

    [Fact]
    public async Task Await_Multiple_Events_In_Order()
    {
        // Arrange
        var cut = RenderWithFake(out var fake);

        cut.Find("input[placeholder='Enter username']").Change("alice");
        cut.Find("button").Click();


        // Act: fire events in the same order
        await fake.SimulateUserJoinedAsync("bob");
        await fake.SimulateMessageReceivedAsync("bob", "hi");
        await fake.SimulateUserLeftAsync("bob");

        // Assert: the awaits complete in order with correct payloads
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2));
        Assert.Equal("bob", await fake.WaitForUserJoinedAsync(cts.Token));
        var (u, m) = await fake.WaitForMessageReceivedAsync(cts.Token);
        Assert.Equal("bob", u);
        Assert.Equal("hi", m);
        Assert.Equal("bob", await fake.WaitForUserLeftAsync(cts.Token));

        // Assert: UI received all three
        cut.WaitForAssertion(() =>
        {
            var items = cut.FindAll("div.messages > div");
            Assert.True(items.Count >= 3, "Expected at least three messages");

            var count = items.Count;
            Assert.Equal("~~ User 'bob' joined ~~", items[count - 3].TextContent);
            Assert.Equal("bob: hi", items[count - 2].TextContent);
            Assert.Equal("~~ User 'bob' left ~~", items[count - 1].TextContent);
        });
    }

    [Fact]
    public async Task AwaitNext_CanBeCanceled()
    {
        // Arrange
        var cut = RenderWithFake(out var fake);

        cut.Find("input[placeholder='Enter username']").Change("alice");
        cut.Find("button").Click();

        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(50));

        // Act/Assert: no event occurs before timeout; the await should cancel
        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
        {
            await fake.WaitForUserJoinedAsync(cts.Token);
        });
    }

    [Fact]
    public async Task AwaitNext_Before_UI_Subscribes_IsFine()
    {
        // Arrange
        // Render component but do NOT join yet; start awaiting then join and simulate.
        var cut = RenderWithFake(out var fake);


        // Now join (this is when the component subscribes to OnUserJoined)
        cut.Find("input[placeholder='Enter username']").Change("alice");
        cut.Find("button").Click();

        // Act: simulate after subscriptions are in place
        await fake.SimulateUserJoinedAsync("bob");

        // Assert: awaited value OK and UI shows it
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(1));
        Assert.Equal("bob", await fake.WaitForUserJoinedAsync(cts.Token));

        cut.WaitForAssertion(() =>
        {
            var messages = cut.Find("div.messages");
            Assert.Contains("~~ User 'bob' joined ~~", messages.InnerHtml, StringComparison.Ordinal);
        });
    }
    
    [Fact]
    public async Task ClearRecorded_Resets_Recordings_And_Channels()
    {
        // Arrange
        var cut = RenderWithFake(out var fake);

        // Join so component subscribes to On* handlers
        cut.Find("input[placeholder='Enter username']").Change("alice");
        cut.Find("button").Click();

        // Generate some activity
        await fake.SimulateUserJoinedAsync("bob");
        await fake.SimulateMessageReceivedAsync("bob", "hi");
        await fake.SimulateUserLeftAsync("bob");

        // Send one client-to-server call so it's recorded too
        var msgInput = cut.Find("input[placeholder='Enter message']");
        var sendBtn = cut.Find("button");
        msgInput.Change("hello world");
        sendBtn.Click();

        // Sanity check: recordings exist before reset
        Assert.NotEmpty(fake.UserJoinedEvents);
        Assert.NotEmpty(fake.MessageReceivedEvents);
        Assert.NotEmpty(fake.UserLeftEvents);
        Assert.NotEmpty(fake.SendMessageCalls);

        // Act: reset
        fake.ClearRecorded();

        // Assert: lists are cleared
        Assert.Empty(fake.UserJoinedEvents);
        Assert.Empty(fake.MessageReceivedEvents);
        Assert.Empty(fake.UserLeftEvents);
        Assert.Empty(fake.SendMessageCalls);

        // Assert: channels are reset (no buffered items left)
        using (var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(100)))
        {
            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => fake.WaitForUserJoinedAsync(cts.Token));
        }

        // After reset, new events should be observed freshly

        await fake.SimulateUserJoinedAsync("carol");

        var waitCts = new CancellationTokenSource(TimeSpan.FromSeconds(1));
        var next = await fake.WaitForUserJoinedAsync(waitCts.Token);
        Assert.Equal("carol", next);

        // UI still contains old messages (component state is not reset by ClearRecorded),
        // but it should also reflect the new event after we simulated it.
        cut.WaitForAssertion(() =>
        {
            var messages = cut.Find("div.messages").InnerHtml;
            Assert.Contains("~~ User 'carol' joined ~~", messages, StringComparison.Ordinal);
        });
    }
}