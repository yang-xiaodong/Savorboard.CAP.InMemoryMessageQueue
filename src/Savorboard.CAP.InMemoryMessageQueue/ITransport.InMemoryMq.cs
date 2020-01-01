using System;
using System.Threading.Tasks;
using DotNetCore.CAP;
using DotNetCore.CAP.Internal;
using DotNetCore.CAP.Messages;
using DotNetCore.CAP.Transport;
using Microsoft.Extensions.Logging;

namespace Savorboard.CAP.InMemoryMessageQueue
{
    internal class InMemoryMqTransport : ITransport
    {
        private readonly InMemoryQueue _queue;
        private readonly ILogger _logger;

        public BrokerAddress BrokerAddress => new BrokerAddress("InMemory", "localhost");

        public InMemoryMqTransport(InMemoryQueue queue, ILogger<InMemoryMqTransport> logger)
        {
            _queue = queue;
            _logger = logger;
        }

        public Task<OperateResult> SendAsync(TransportMessage message)
        {
            try
            {
                _queue.Send(message);

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