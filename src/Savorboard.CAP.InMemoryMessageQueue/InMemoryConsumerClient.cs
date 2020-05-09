using System;
using System.Collections.Generic;
using System.Threading;
using DotNetCore.CAP.Messages;
using DotNetCore.CAP.Transport;
using Microsoft.Extensions.Logging;

namespace Savorboard.CAP.InMemoryMessageQueue
{
    internal sealed class InMemoryConsumerClient : IConsumerClient
    {
        private readonly ILogger _logger;
        private readonly InMemoryQueue _queue;
        private readonly string _groupId;

        public InMemoryConsumerClient(
            ILogger logger,
            InMemoryQueue queue,
            string groupId)
        {
            _logger = logger;
            _queue = queue;
            _groupId = groupId;
        }

        public event EventHandler<TransportMessage> OnMessageReceived;

        public event EventHandler<LogMessageEventArgs> OnLog;

        public BrokerAddress BrokerAddress => new BrokerAddress("InMemory", "localhost");

        public void Subscribe(IEnumerable<string> topics)
        {
            if (topics == null) throw new ArgumentNullException(nameof(topics));

            foreach (var topic in topics)
            {
                _queue.Subscribe(_groupId, OnConsumerReceived, topic);

                _logger.LogInformation($"InMemory message queue initialize the topic: {_groupId} {topic}");
            }
        }

        public void Listening(TimeSpan timeout, CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                cancellationToken.WaitHandle.WaitOne(timeout);
            }
        }

        public void Commit(object sender)
        {
            // ignore
        }

        public void Reject(object sender)
        {
            // ignore
        }

        public void Dispose()
        {
            _queue.ClearSubscriber();
        }

        #region private methods

        private void OnConsumerReceived(TransportMessage e)
        {
            OnMessageReceived?.Invoke(null, e);
        }
        #endregion private methods
    }
}