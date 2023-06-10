//HintName: TestHubClient.g.cs
using SignalRGen.Generator.Tests.TestData;
using Microsoft.AspNetCore.SignalR.Client;

namespace iCMS.FM.SignalR.Contracts;
public class TestHubClient : HubClient
{
    public TestHubClient(HubConnection hubConnection) : base(hubConnection)
    {
    }

    public Func<IEnumerable<CustomTypeDto>, Task> OnReceiveCustomTypeUpdate = default;
    private Task ReceiveCustomTypeUpdateHandler(IEnumerable<CustomTypeDto> customTypes)
    {
        return OnReceiveCustomTypeUpdate.Invoke(customTypes);
    }

    public Func<string, int, Task> OnReceiveFooUpdate = default;
    private Task ReceiveFooUpdateHandler(string bar, int bass)
    {
        return OnReceiveFooUpdate.Invoke(bar);
    }

    protected override void RegisterHubMethods()
    {
        HubConnection.On<IEnumerable<CustomTypeDto>>("ReceiveCustomTypeUpdate", ReceiveCustomTypeUpdateHandler);
        HubConnection.On<string, int>("ReceiveFooUpdate", ReceiveFooUpdateHandler);
    }
}