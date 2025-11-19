using System.Text;
using DotNetCore.CAP.Internal;
using DotNetCore.CAP.Messages;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Savorboard.CAP.InMemoryMessageQueue;
using Xunit;
using Xunit.Abstractions;

namespace InMemoryQueueTest;

/// <summary>
/// Tests for the InMemoryConsumerClient class.
/// </summary>
public class ConsumerClientTests(ITestOutputHelper output)
{
    /// <summary>
    /// Tests that InvalidOperationException is thrown when queue receives a message for a non-subscribed topic.
    /// </summary>
    [Fact]
    public void QueueNotSubscribeTest()
    {
        var logger = NullLoggerFactory.Instance.CreateLogger<InMemoryQueue>();
        const string groupId = "test-group";
        const string topic = "test-topic";
        const string content = "test content";

        var queue = new InMemoryQueue(logger);
        queue.Subscribe(groupId, [topic]);
        var headers = new Dictionary<string, string?>();
        var messageId = new SnowflakeId().NextId().ToString();
        headers.Add(Headers.MessageId, messageId);
        headers.Add(Headers.MessageName, topic + "-assert");

        Assert.Throws<InvalidOperationException>(() => 
            queue.Send(new TransportMessage(headers, Encoding.UTF8.GetBytes(content))));
    }

    /// <summary>
    /// Tests that messages are correctly sent and received through the queue.
    /// </summary>
    [Fact]
    public void SendMessageTest()
    {
        var logger = NullLoggerFactory.Instance.CreateLogger<InMemoryQueue>();
        const string groupId = "test-group";
        const string topic = "test-topic";
        const string content = "test content";

        var queue = new InMemoryQueue(logger);
        var headers = new Dictionary<string, string?>();

        var messageId = new SnowflakeId().NextId().ToString();
        headers.Add(Headers.MessageId, messageId);
        headers.Add(Headers.MessageName, topic);
        headers.Add(Headers.Type, typeof(string).FullName!);
        headers.Add(Headers.SentTime, DateTimeOffset.Now.ToString());
        if (headers.TryAdd(Headers.CorrelationId, messageId))
        {
            headers.Add(Headers.CorrelationSequence, 0.ToString());
        }

        var transportMsg = new TransportMessage(headers, Encoding.UTF8.GetBytes(content));

        var clientLogger = NullLoggerFactory.Instance.CreateLogger(typeof(InMemoryConsumerClient).Name);
        var reset = new ManualResetEventSlim(false);
        var client = new InMemoryConsumerClient(clientLogger, queue, groupId, 1)
        {
            OnMessageCallback = (x, obj) =>
            {
                output.WriteLine($"Received message: {Encoding.UTF8.GetString(x.Body.ToArray())}");
                Assert.Equal(content, Encoding.UTF8.GetString(x.Body.ToArray()));
                reset.Set();
                return Task.CompletedTask;
            }
        };

        _ = client.SubscribeAsync([topic]);

        _ = Task.Run(() => client.ListeningAsync(TimeSpan.FromSeconds(10), default));

        queue.Send(transportMsg);

        reset.Wait();
    }
}
