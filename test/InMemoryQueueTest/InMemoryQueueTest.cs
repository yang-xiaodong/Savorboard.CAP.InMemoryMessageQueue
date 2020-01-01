using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using DotNetCore.CAP.Internal;
using DotNetCore.CAP.Messages;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Savorboard.CAP.InMemoryMessageQueue;
using Xunit;

namespace InMemoryQueueTest
{
    public class InMemoryQueueTest
    {
        [Fact]
        public void SubscribeTest()
        {
            var logger = NullLoggerFactory.Instance.CreateLogger<InMemoryQueue>();
            var queue = new InMemoryQueue(logger);
            queue.Subscribe("groupid", x => new TransportMessage(null, null), "test-topic");
        }

        [Fact]
        public void SendTest()
        {
            var logger = NullLoggerFactory.Instance.CreateLogger<InMemoryQueue>();
            var queue = new InMemoryQueue(logger);
            var topic = "test-topic";
            var content = "test content";

           var headers = new Dictionary<string, string>();

            var messageId = SnowflakeId.Default().NextId().ToString();
            headers.Add(Headers.MessageId, messageId);
            headers.Add(Headers.MessageName, topic);
            headers.Add(Headers.Type, typeof(string).FullName);
            headers.Add(Headers.SentTime, DateTimeOffset.Now.ToString());
            if (!headers.ContainsKey(Headers.CorrelationId))
            {
                headers.Add(Headers.CorrelationId, messageId);
                headers.Add(Headers.CorrelationSequence, 0.ToString());
            }

            var transportMsg = new TransportMessage(headers, Encoding.UTF8.GetBytes(content));

            ManualResetEventSlim reset = new ManualResetEventSlim(false);
            queue.Subscribe("groupid", x =>
            {
                Assert.Equal(content,Encoding.UTF8.GetString(x.Body));
                reset.Set();
            }, topic);
            queue.Send(transportMsg);
            reset.Wait();
        }
    }
}
