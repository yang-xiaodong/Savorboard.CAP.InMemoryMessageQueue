using DotNetCore.CAP;
using DotNetCore.CAP.Internal;
using DotNetCore.CAP.Messages;
using DotNetCore.CAP.Transport;
using Microsoft.Extensions.Logging;

namespace Savorboard.CAP.InMemoryMessageQueue;

/// <summary>
/// Transport implementation for in-memory message queue.
/// </summary>
internal class InMemoryMqTransport(InMemoryQueue queue, ILogger<InMemoryMqTransport> logger) : ITransport
{
    private readonly ILogger _logger = logger;

    /// <summary>
    /// Gets the broker address information.
    /// </summary>
    public BrokerAddress BrokerAddress => new("InMemory", "localhost");

    /// <summary>
    /// Sends a transport message asynchronously.
    /// </summary>
    /// <param name="message">The transport message to send</param>
    /// <returns>A task that returns the operation result</returns>
    public Task<OperateResult> SendAsync(TransportMessage message)
    {
        try
        {
            queue.Send(message);
            _logger.LogDebug("Event message [{MessageName}] has been published.", message.GetName());
            return Task.FromResult(OperateResult.Success);
        }
        catch (Exception ex)
        {
            var wrapperEx = new PublisherSentFailedException(ex.Message, ex);
            return Task.FromResult(OperateResult.Failed(wrapperEx));
        }
    }
}