using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DotNetCore.CAP.Internal;
using DotNetCore.CAP.Messages;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Savorboard.CAP.InMemoryMessageQueue;
using Xunit;
using Xunit.Abstractions;

namespace InMemoryQueueTest
{
    public class ConsumerClientTests(ITestOutputHelper output)
    {
        [Fact]
        public void QueueNotSubscribeTest()
        {
            var logger = NullLoggerFactory.Instance.CreateLogger<InMemoryQueue>();
            var groupId = "test-group";
            var topic = "test-topic";
            var content = "test content";

            var queue = new InMemoryQueue(logger);
            queue.Subscribe(groupId, [topic]);
            var headers = new Dictionary<string, string>();
            var messageId = new SnowflakeId().NextId().ToString();
            headers.Add(Headers.MessageId, messageId);
            headers.Add(Headers.MessageName, topic + "-assert");
            Assert.Throws<InvalidOperationException>(() => queue.Send(new TransportMessage(headers, Encoding.UTF8.GetBytes(content))));
        }

        [Fact]
        public void SendMessageTest()
        {
            var logger = NullLoggerFactory.Instance.CreateLogger<InMemoryQueue>();
            var groupId = "test-group";
            var topic = "test-topic";
            var content = "test content";

            var queue = new InMemoryQueue(logger);
            var headers = new Dictionary<string, string>();

            var messageId = new SnowflakeId().NextId().ToString();
            headers.Add(Headers.MessageId, messageId);
            headers.Add(Headers.MessageName, topic);
            headers.Add(Headers.Type, typeof(string).FullName);
            headers.Add(Headers.SentTime, DateTimeOffset.Now.ToString());
            if (headers.TryAdd(Headers.CorrelationId, messageId))
            {
                headers.Add(Headers.CorrelationSequence, 0.ToString());
            }
            var transportMsg = new TransportMessage(headers, Encoding.UTF8.GetBytes(content));

            ManualResetEventSlim reset = new(false);
            var client = new InMemoryConsumerClient(logger, queue, groupId, 1)
            {
                OnMessageCallback = (x, obj) =>
                {
                    output.WriteLine($"Received message: {Encoding.UTF8.GetString(x.Body.ToArray())}");
                    Assert.Equal(content, Encoding.UTF8.GetString(x.Body.ToArray()));
                    reset.Set();
                    return Task.CompletedTask;
                }
            };

            client.Subscribe([topic]);

            Task.Run(() => client.Listening(TimeSpan.FromSeconds(10), default));

            queue.Send(transportMsg);

            reset.Wait();
        }
    }
}
