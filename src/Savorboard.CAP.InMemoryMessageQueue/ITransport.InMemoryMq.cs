using System;
using System.Threading.Tasks;
using DotNetCore.CAP;
using DotNetCore.CAP.Internal;
using DotNetCore.CAP.Messages;
using DotNetCore.CAP.Transport;
using Microsoft.Extensions.Logging;

namespace Savorboard.CAP.InMemoryMessageQueue
{
    internal class InMemoryMqTransport(InMemoryQueue queue, ILogger<InMemoryMqTransport> logger) : ITransport
    {
        private readonly ILogger _logger = logger;

        public BrokerAddress BrokerAddress => new("InMemory", "localhost");

        public Task<OperateResult> SendAsync(TransportMessage message)
        {
            try
            {
                queue.Send(message);

                _logger.LogDebug($"Event message [{message.GetName()}] has been published.");

                return Task.FromResult(OperateResult.Success);
            }
            catch (Exception ex)
            {
                var wrapperEx = new PublisherSentFailedException(ex.Message, ex);

                return Task.FromResult(OperateResult.Failed(wrapperEx));
            }
        }
    }
}