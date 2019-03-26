using System.Threading;
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
            queue.Subscribe("groupid", x =>
            {
                x.Content = "Content";
                x.Group = "Group";
                x.Name = "Name";
            }, "test-topic");
        }

        [Fact]
        public void SendTest()
        {
            var logger = NullLoggerFactory.Instance.CreateLogger<InMemoryQueue>();
            var queue = new InMemoryQueue(logger);
            var topic = "test-topic";
            var content = "test content";

            ManualResetEventSlim reset = new ManualResetEventSlim(false);
            queue.Subscribe("groupid", x =>
            {
                Assert.Equal(content, x.Content);
                reset.Set();
            }, topic);
            queue.Send(topic, content);
            reset.Wait();
        }
    }
}
