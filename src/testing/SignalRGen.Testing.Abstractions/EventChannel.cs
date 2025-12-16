using System.Threading.Channels;

namespace SignalRGen.Testing.Abstractions;

/// <summary>
/// Represents a channel for publishing and consuming events of a specified type.
/// </summary>
/// <typeparam name="TEvent">The type of event to be published and consumed.</typeparam>
public class EventChannel<TEvent>
{
    private Channel<TEvent> _channel = Create<TEvent>();
    
    private static Channel<T> Create<T>() =>
        Channel.CreateUnbounded<T>(new UnboundedChannelOptions
        {
            SingleReader = true,
            SingleWriter = false,
            AllowSynchronousContinuations = false
        });

    /// <summary>
    /// Publishes an event of the specified type to the channel asynchronously.
    /// </summary>
    /// <param name="value">The event to be published.</param>
    /// <param name="ct">An optional <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    public Task PublishAsync(TEvent value, CancellationToken ct = default)
        => _channel.Writer.WriteAsync(value, ct).AsTask();

    /// <summary>
    /// Asynchronously reads the next event from the channel.
    /// </summary>
    /// <param name="ct">An optional <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task{TResult}"/> that represents the asynchronous operation, containing the next event of type <typeparamref name="TEvent"/>.</returns>
    public Task<TEvent> WaitNextAsync(CancellationToken ct = default)
        => _channel.Reader.ReadAsync(ct).AsTask();

    /// <summary>
    /// Resets the channel by discarding all pending events and creating a new channel instance.
    /// </summary>
    /// <remarks>
    /// The current writer is marked as completed to ensure all pending readers can finish consuming any buffered events.
    /// </remarks>
    public void Reset()
    {
        var old = _channel;
        _channel = Create<TEvent>();
        // Best-effort: complete the old writer so pending readers finish consuming whatever was buffered.
        old.Writer.TryComplete();
    }
}