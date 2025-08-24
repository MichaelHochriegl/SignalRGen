using Microsoft.AspNetCore.Http.Connections.Client;
using Microsoft.AspNetCore.SignalR.Client;
using SignalRGen.Example.Contracts;

namespace SignalRGen.Example.Client.BlazorServer.Tests.ManualFakes;

// This is the "generated" fake.
public sealed class FakeChatHubContractClient : ChatHubContractClient
{
    // TODO: Do we need multiple locks?
    private readonly Lock _lock = new();
    
    public IReadOnlyList<string> SendMessageCalls
    {
        get { lock (_lock) return _sendMessageCalls.ToList(); }
    }
    private readonly List<string> _sendMessageCalls = new();

    // Proposal:
    // We can use this to allow custom behavior for the test execution.
    public Func<string, CancellationToken, Task>? SendMessageHandler { get; set; }
    
    public IReadOnlyList<string> UserJoinedEvents
    {
        get { lock (_lock) return _userJoinedEvents.ToList(); }
    }
    private readonly List<string> _userJoinedEvents = new();
    private readonly EventChannel<string> _userJoinedChannel = new();

    public IReadOnlyList<string> UserLeftEvents
    {
        get { lock (_lock) return _userLeftEvents.ToList(); }
    }
    private readonly List<string> _userLeftEvents = new();
    private readonly EventChannel<string> _userLeftChannel = new();

    public IReadOnlyList<(string User, string Message)> MessageReceivedEvents
    {
        get { lock (_lock) return _messageReceivedEvents.ToList(); }
    }
    private readonly List<(string User, string Message)> _messageReceivedEvents = new();
    private readonly EventChannel<(string User, string Message)> _messageReceivedChannel = new();

    // Proposal:
    // We can use this to make the fake strict so that it throws if it receives an unexpected call.
    public bool Strict { get; set; }

    public FakeChatHubContractClient(
        Action<IHubConnectionBuilder>? hubConnectionBuilderConfiguration = null,
        Uri? baseHubUri = null,
        Action<HttpConnectionOptions>? httpConnectionOptionsConfiguration = null)
        : base(hubConnectionBuilderConfiguration, baseHubUri ?? new Uri("http://localhost/"), httpConnectionOptionsConfiguration)
    {
    }

    protected override void RegisterHubMethods()
    {
        // We don't need to do anything here, as we are the fake.
    }

    public override Task StartAsync(
        Dictionary<string, string>? queryStrings = null,
        Dictionary<string, string>? headers = null,
        CancellationToken cancellationToken = default)
    {
        // TODO: This is ugly, but otherwise we can't call client-to-server methods, they error out when the `_hubConnection` is null.
        _hubConnection = new HubConnectionBuilder().WithUrl("http://localhost").Build();
        return Task.CompletedTask;
    }

    public override Task StopAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    
    protected override Task InvokeCoreAsync(string methodName, object?[] args, CancellationToken cancellationToken)
    {
        // TODO: This will get ugly with more events, maybe there is a better way to do this?
        if (string.Equals(methodName, "SendMessage", StringComparison.Ordinal))
        {
            var message = (string)args[0]!;
            lock (_lock) _sendMessageCalls.Add(message);

            if (SendMessageHandler is not null) return SendMessageHandler(message, cancellationToken);
            if (Strict) throw new InvalidOperationException("No behavior configured for SendMessage.");
            return Task.CompletedTask;
        }

        if (Strict) throw new NotSupportedException($"Method '{methodName}' is not supported by the fake.");
        return Task.CompletedTask;
    }
    
    public async Task SimulateUserJoinedAsync(string user, CancellationToken ct = default)
    {
        lock (_lock) _userJoinedEvents.Add(user);
        await _userJoinedChannel.PublishAsync(user, ct).ConfigureAwait(false);

        var handler = OnUserJoined;
        if (handler is not null)
        {
            await handler.Invoke(user).ConfigureAwait(false);
        }
    }

    public async Task SimulateUserLeftAsync(string user, CancellationToken ct = default)
    {
        lock (_lock) _userLeftEvents.Add(user);
        await _userLeftChannel.PublishAsync(user, ct).ConfigureAwait(false);

        var handler = OnUserLeft;
        if (handler is not null)
        {
            await handler.Invoke(user).ConfigureAwait(false);
        }
    }

    public async Task SimulateMessageReceivedAsync(string user, string message, CancellationToken ct = default)
    {
        lock (_lock) _messageReceivedEvents.Add((user, message));
        await _messageReceivedChannel.PublishAsync((user, message), ct).ConfigureAwait(false);

        var handler = OnMessageReceived;
        if (handler is not null)
        {
            await handler.Invoke(user, message).ConfigureAwait(false);
        }
    }
    
    public async Task<string> WaitForUserJoinedAsync(CancellationToken ct = default)
        => await _userJoinedChannel.WaitNextAsync(ct).ConfigureAwait(false);

    public async Task<string> WaitForUserLeftAsync(CancellationToken ct = default)
        => await _userLeftChannel.WaitNextAsync(ct).ConfigureAwait(false);

    public async Task<(string User, string Message)> WaitForMessageReceivedAsync(CancellationToken ct = default)
        => await _messageReceivedChannel.WaitNextAsync(ct).ConfigureAwait(false);
    
    public void ClearRecorded()
    {
        lock (_lock)
        {
            _sendMessageCalls.Clear();
            _userJoinedEvents.Clear();
            _userLeftEvents.Clear();
            _messageReceivedEvents.Clear();
        }
        _userJoinedChannel.Reset();
        _userLeftChannel.Reset();
        _messageReceivedChannel.Reset();
    }

}