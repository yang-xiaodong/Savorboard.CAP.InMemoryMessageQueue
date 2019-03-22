// Copyright (c) .NET Core Community. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DotNetCore.CAP;
using Microsoft.Extensions.Logging;

namespace Savorboard.CAP.InMemoryMessageQueue
{
    internal sealed class InMemoryConsumerClient : IConsumerClient
    {
        private readonly ILogger _logger;
        private readonly string _subscriptionName;

        public readonly ConcurrentDictionary<string, Queue<byte[]>> TopicQueue;

        public InMemoryConsumerClient(
            ILogger logger,
            string subscriptionName)
        {
            _logger = logger;
            _subscriptionName = subscriptionName;
            
            TopicQueue = new ConcurrentDictionary<string, Queue<byte[]>>();

            InitAzureServiceBusClient().GetAwaiter().GetResult();
        }

        public event EventHandler<MessageContext> OnMessageReceived;

        public event EventHandler<LogMessageEventArgs> OnLog;

        public string ServersAddress => string.Empty;

        public void Subscribe(IEnumerable<string> topics)
        {
            if (topics == null) throw new ArgumentNullException(nameof(topics));

            foreach (var topic in topics)
            {
                TopicQueue.AddOrUpdate(topic, new Queue<byte[]>(), (x, y) => new Queue<byte[]>());

                _logger.LogInformation($"InMemory message queue initialize the topic: {topic}");
            }
        }

        public void Listening(TimeSpan timeout, CancellationToken cancellationToken)
        {
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();
                cancellationToken.WaitHandle.WaitOne(timeout);
            }

            // ReSharper disable once FunctionNeverReturns
        }

        public void Commit()
        {
            // ignore
        }

        public void Reject()
        {
            // ignore
        }

        public void Dispose()
        {
            
        }

        #region private methods

        private async Task InitAzureServiceBusClient()
        {
            
        }

        //private Task OnConsumerReceived(Message message, CancellationToken token)
        //{
            
        //    var context = new MessageContext
        //    {
        //        Group = _subscriptionName,
        //        Name = message.Label,
        //        Content = Encoding.UTF8.GetString(message.Body)
        //    };

        //    OnMessageReceived?.Invoke(null, context);

        //    return Task.CompletedTask;
        //}

        //private Task OnExceptionReceived(ExceptionReceivedEventArgs args)
        //{
        //    var context = args.ExceptionReceivedContext;
        //    var exceptionMessage =
        //        $"- Endpoint: {context.Endpoint}\r\n" +
        //        $"- Entity Path: {context.EntityPath}\r\n" +
        //        $"- Executing Action: {context.Action}\r\n" +
        //        $"- Exception: {args.Exception}";

        //    var logArgs = new LogMessageEventArgs
        //    {
        //        LogType = MqLogType.ExceptionReceived,
        //        Reason = exceptionMessage
        //    };

        //    OnLog?.Invoke(null, logArgs);

        //    return Task.CompletedTask;
        //}

        #endregion private methods
    }
}