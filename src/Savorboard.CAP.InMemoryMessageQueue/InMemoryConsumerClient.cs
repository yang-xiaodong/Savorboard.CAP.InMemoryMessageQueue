using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
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
        private readonly byte _groupConcurrent;
        private readonly BlockingCollection<TransportMessage> _messageQueue = new();
        private readonly SemaphoreSlim _semaphore;

        public InMemoryConsumerClient(ILogger logger, InMemoryQueue queue, string groupId, byte groupConcurrent)
        {
            _logger = logger;
            _queue = queue;
            _groupId = groupId;
            _groupConcurrent = groupConcurrent;
            _semaphore = new SemaphoreSlim(groupConcurrent);
            _queue.RegisterConsumerClient(groupId, this);
        }

        public Func<TransportMessage, object, Task> OnMessageCallback { get; set; }

        public Action<LogMessageEventArgs> OnLogCallback { get; set; }

        public BrokerAddress BrokerAddress => new("InMemory", "localhost");

        public void Subscribe(IEnumerable<string> topics)
        {
            if (topics == null) throw new ArgumentNullException(nameof(topics));

            _queue.Subscribe(_groupId, topics);
        }

        public void AddSubscribeMessage(TransportMessage message)
        {
            _messageQueue.Add(message);
        }

        public void Listening(TimeSpan timeout, CancellationToken cancellationToken)
        {
            foreach (var message in _messageQueue.GetConsumingEnumerable(cancellationToken))
            {
                if (_groupConcurrent > 0)
                {
                    _semaphore.Wait(cancellationToken);
                    Task.Run(() => OnMessageCallback?.Invoke(message, null), cancellationToken).ConfigureAwait(false);
                }
                else
                {
                    OnMessageCallback?.Invoke(message, null).ConfigureAwait(false).GetAwaiter().GetResult();
                }
            }
        }

        public void Commit(object sender)
        {
            _semaphore.Release();
        }

        public void Reject(object sender)
        {
            _semaphore.Release();
        }

        public void Dispose()
        {
            _queue.Unsubscribe(_groupId);
        }
    }
}