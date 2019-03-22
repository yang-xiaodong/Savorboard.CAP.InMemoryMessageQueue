using System;
using System.Threading.Tasks;
using DotNetCore.CAP;
using DotNetCore.CAP.Internal;
using DotNetCore.CAP.Processor.States;
using Microsoft.Extensions.Logging;

namespace Savorboard.CAP.InMemoryMessageQueue
{
    internal class InMemoryMqPublishMessageSender : BasePublishMessageSender
    {
        private readonly InMemoryQueue _queue;
        private readonly ILogger _logger;

        public InMemoryMqPublishMessageSender(InMemoryQueue queue,
            ILogger<InMemoryMqPublishMessageSender> logger,
            CapOptions options,
            IStateChanger stateChanger,
            IStorageConnection connection)
            : base(logger, options, connection, stateChanger)
        {
            _queue = queue;
            _logger = logger;
        }

        public override async Task<OperateResult> PublishAsync(string keyName, string content)
        {
            try
            {
                _queue.Send(keyName, content);

                _logger.LogDebug($"Event message [{keyName}] has been published.");

                return await Task.FromResult(OperateResult.Success);
            }
            catch (Exception ex)
            {
                var wrapperEx = new PublisherSentFailedException(ex.Message, ex);

                return await Task.FromResult(OperateResult.Failed(wrapperEx));
            }
        }
    }
}