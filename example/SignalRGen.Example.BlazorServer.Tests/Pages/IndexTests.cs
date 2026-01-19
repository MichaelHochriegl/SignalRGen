using Bunit;
using Microsoft.Extensions.DependencyInjection;
using SignalRGen.Example.Contracts;
using SignalRGen.Example.Contracts.TestFakes;
using Index = SignalRGen.Example.Client.BlazorServer.Pages.Index;

namespace SignalRGen.Example.BlazorServer.Tests.Pages;

public class IndexTests : BunitContext, IAsyncLifetime
{
    public IndexTests()
    {
        Services.AddSingleton<ExampleHubClient, FakeExampleHubClient>();
    }
    
    [Fact]
    public async Task Clicking_SendNoParameters_Calls_Hub_Method()
    {
        // Arrange
        await using var fakeClient = await GetFake();
        var cut = Render<Index>();

        // Act
        await cut.Find("#send-no-params-button").ClickAsync();

        // Assert
        Assert.Equal(1, fakeClient.NoParametersCallCount);
    }
    
    private async Task<FakeExampleHubClient> GetFake()
    {
        FakeExampleHubClient? fakeClient = null;
        try
        {
            fakeClient = Services.GetRequiredService<ExampleHubClient>() as FakeExampleHubClient;
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