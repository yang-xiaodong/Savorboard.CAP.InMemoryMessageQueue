using System.Collections.Concurrent;
using DotNetCore.CAP.Messages;
using DotNetCore.CAP.Transport;
using Microsoft.Extensions.Logging;

namespace Savorboard.CAP.InMemoryMessageQueue;

/// <summary>
/// Consumer client for in-memory message queue.
/// </summary>
internal sealed class InMemoryConsumerClient : IConsumerClient
{
    private readonly InMemoryQueue _queue;
    private readonly string _groupId;
    private readonly byte _groupConcurrent;
    private readonly BlockingCollection<TransportMessage> _messageQueue = new();
    private readonly SemaphoreSlim _semaphore;

    /// <summary>
    /// Initializes a new instance of the InMemoryConsumerClient class.
    /// </summary>
    /// <param name="logger">The logger instance</param>
    /// <param name="queue">The in-memory queue instance</param>
    /// <param name="groupId">The consumer group ID</param>
    /// <param name="groupConcurrent">The concurrency level for the group</param>
    public InMemoryConsumerClient(ILogger logger, InMemoryQueue queue, string groupId, byte groupConcurrent)
    {
        _queue = queue;
        _groupId = groupId;
        _groupConcurrent = groupConcurrent;
        _semaphore = new SemaphoreSlim(groupConcurrent);
        _queue.RegisterConsumerClient(groupId, this);
    }

    /// <summary>
    /// Gets or sets the callback function to handle received messages.
    /// </summary>
    public Func<TransportMessage, object?, Task>? OnMessageCallback { get; set; }

    /// <summary>
    /// Gets or sets the callback function for logging events.
    /// </summary>
    public Action<LogMessageEventArgs>? OnLogCallback { get; set; }

    /// <summary>
    /// Gets the broker address information.
    /// </summary>
    public BrokerAddress BrokerAddress => new("InMemory", "localhost");

    /// <summary>
    /// Subscribes to the specified topics.
    /// </summary>
    /// <param name="topics">The list of topics to subscribe to</param>
    /// <returns>A completed task</returns>
    /// <exception cref="ArgumentNullException">Thrown when topics is null</exception>
    public Task SubscribeAsync(IEnumerable<string> topics)
    {
        ArgumentNullException.ThrowIfNull(topics);
        _queue.Subscribe(_groupId, topics);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Adds a message to the message queue for processing.
    /// </summary>
    /// <param name="message">The transport message to add</param>
    public void AddSubscribeMessage(TransportMessage message)
    {
        _messageQueue.Add(message);
    }

    /// <summary>
    /// Listens for messages with the specified timeout.
    /// </summary>
    /// <param name="timeout">The timeout for listening</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>A task representing the listening operation</returns>
    public Task ListeningAsync(TimeSpan timeout, CancellationToken cancellationToken)
    {
        foreach (var message in _messageQueue.GetConsumingEnumerable(cancellationToken))
        {
            if (_groupConcurrent > 0)
            {
                _semaphore.Wait(cancellationToken);
                _ = Task.Run(() => OnMessageCallback?.Invoke(message, null), cancellationToken).ConfigureAwait(false);
            }
            else
            {
                OnMessageCallback?.Invoke(message, null).ConfigureAwait(false).GetAwaiter().GetResult();
            }
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Commits the processing of a message.
    /// </summary>
    /// <param name="sender">The sender object</param>
    /// <returns>A completed task</returns>
    public Task CommitAsync(object? sender)
    {
        _semaphore.Release();
        return Task.CompletedTask;
    }

    /// <summary>
    /// Rejects the processing of a message.
    /// </summary>
    /// <param name="sender">The sender object</param>
    /// <returns>A completed task</returns>
    public Task RejectAsync(object? sender)
    {
        _semaphore.Release();
        return Task.CompletedTask;
    }

    /// <summary>
    /// Disposes the consumer client and unsubscribes from the queue.
    /// </summary>
    /// <returns>A value task representing the disposal</returns>
    public ValueTask DisposeAsync()
    {
        _queue.Unsubscribe(_groupId);
        return ValueTask.CompletedTask;
    }
}