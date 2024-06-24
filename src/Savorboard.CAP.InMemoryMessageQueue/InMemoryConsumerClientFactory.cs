using DotNetCore.CAP.Transport;
using Microsoft.Extensions.Logging;

namespace Savorboard.CAP.InMemoryMessageQueue
{
    internal sealed class InMemoryConsumerClientFactory(ILoggerFactory loggerFactory, InMemoryQueue queue) : IConsumerClientFactory
    {
        public IConsumerClient Create(string groupId, byte groupConcurrent)
        {
            var logger = loggerFactory.CreateLogger(typeof(InMemoryConsumerClient));
            return new InMemoryConsumerClient(logger, queue, groupId, groupConcurrent);
        }
    }
}