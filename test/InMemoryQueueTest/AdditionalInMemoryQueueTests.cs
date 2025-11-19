using System.Text;
using DotNetCore.CAP.Internal;
using DotNetCore.CAP.Messages;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Savorboard.CAP.InMemoryMessageQueue;
using Xunit;

namespace InMemoryQueueTest;

public class AdditionalInMemoryQueueTests
{
    [Fact]
    public void Transport_SendAsync_Fails_When_No_Subscription()
    {
        var queue = new InMemoryQueue(NullLoggerFactory.Instance.CreateLogger<InMemoryQueue>());
        var transport = new InMemoryMqTransport(queue, NullLoggerFactory.Instance.CreateLogger<InMemoryMqTransport>());
        var headers = new Dictionary<string, string?>
        {
            [Headers.MessageId] = new SnowflakeId().NextId().ToString(),
            [Headers.MessageName] = "not-subscribed-topic"
        };
        var message = new TransportMessage(headers, Encoding.UTF8.GetBytes("payload"));

        var result = transport.SendAsync(message).GetAwaiter().GetResult();

        Assert.False(result.Succeeded);
        Assert.NotNull(result.Exception);
        Assert.IsType<PublisherSentFailedException>(result.Exception);
    }

    [Fact]
    public void Message_Delivered_To_All_Subscribed_Groups()
    {
        var queue = new InMemoryQueue(NullLoggerFactory.Instance.CreateLogger<InMemoryQueue>());
        const string topic = "multi-group-topic";

        var logger = NullLoggerFactory.Instance.CreateLogger("client");
        var receivedByGroup1 = false;
        var receivedByGroup2 = false;
        var reset = new ManualResetEventSlim(false);

        var client1 = new InMemoryConsumerClient(logger, queue, "group-1", 1)
        {
            OnMessageCallback = (msg, _) =>
            {
                receivedByGroup1 = true;
                if (receivedByGroup1 && receivedByGroup2) reset.Set();
                return Task.CompletedTask;
            }
        };
        var client2 = new InMemoryConsumerClient(logger, queue, "group-2", 1)
        {
            OnMessageCallback = (msg, _) =>
            {
                receivedByGroup2 = true;
                if (receivedByGroup1 && receivedByGroup2) reset.Set();
                return Task.CompletedTask;
            }
        };

        _ = client1.SubscribeAsync([topic]);
        _ = client2.SubscribeAsync([topic]);

        _ = Task.Run(() => client1.ListeningAsync(TimeSpan.FromSeconds(5), default));
        _ = Task.Run(() => client2.ListeningAsync(TimeSpan.FromSeconds(5), default));

        var headers = new Dictionary<string, string?>
        {
            [Headers.MessageId] = new SnowflakeId().NextId().ToString(),
            [Headers.MessageName] = topic
        };
        queue.Send(new TransportMessage(headers, Encoding.UTF8.GetBytes("data")));

        reset.Wait(TimeSpan.FromSeconds(5));
        Assert.True(receivedByGroup1);
        Assert.True(receivedByGroup2);
    }

    [Fact]
    public void Duplicate_Subscription_Does_Not_Duplicate_Message()
    {
        var queue = new InMemoryQueue(NullLoggerFactory.Instance.CreateLogger<InMemoryQueue>());
        const string topic = "duplicate-subscription";
        var logger = NullLoggerFactory.Instance.CreateLogger("client");
        var callCount = 0;
        var reset = new ManualResetEventSlim(false);

        var client = new InMemoryConsumerClient(logger, queue, "dup-group", 1)
        {
            OnMessageCallback = (msg, _) =>
            {
                Interlocked.Increment(ref callCount);
                reset.Set();
                return Task.CompletedTask;
            }
        };

        _ = client.SubscribeAsync([topic]);
        _ = client.SubscribeAsync([topic]); // subscribe same topic again

        _ = Task.Run(() => client.ListeningAsync(TimeSpan.FromSeconds(5), default));

        var headers = new Dictionary<string, string?>
        {
            [Headers.MessageId] = new SnowflakeId().NextId().ToString(),
            [Headers.MessageName] = topic
        };
        queue.Send(new TransportMessage(headers, Encoding.UTF8.GetBytes("payload")));

        reset.Wait(TimeSpan.FromSeconds(5));
        Assert.Equal(1, callCount);
    }

    [Fact]
    public void Disposed_Client_Does_Not_Receive_Further_Messages()
    {
        var queue = new InMemoryQueue(NullLoggerFactory.Instance.CreateLogger<InMemoryQueue>());
        const string topic = "dispose-test";
        var logger = NullLoggerFactory.Instance.CreateLogger("client");
        var callCount = 0;

        var client = new InMemoryConsumerClient(logger, queue, "dispose-group", 1)
        {
            OnMessageCallback = (msg, _) =>
            {
                Interlocked.Increment(ref callCount);
                return Task.CompletedTask;
            }
        };

        _ = client.SubscribeAsync([topic]);
        _ = Task.Run(() => client.ListeningAsync(TimeSpan.FromSeconds(2), default));

        // First message should be received
        var headers1 = new Dictionary<string, string?>
        {
            [Headers.MessageId] = new SnowflakeId().NextId().ToString(),
            [Headers.MessageName] = topic
        };
        queue.Send(new TransportMessage(headers1, Encoding.UTF8.GetBytes("one")));
        Thread.Sleep(200); // allow processing
        Assert.Equal(1, callCount);

        // Dispose client
        client.DisposeAsync().GetAwaiter().GetResult();

        // Second message should NOT be received (client unregistered)
        var headers2 = new Dictionary<string, string?>
        {
            [Headers.MessageId] = new SnowflakeId().NextId().ToString(),
            [Headers.MessageName] = topic
        };
        queue.Send(new TransportMessage(headers2, Encoding.UTF8.GetBytes("two")));
        Thread.Sleep(200);
        Assert.Equal(1, callCount);
    }

    [Fact]
    public void Concurrency_Allows_Multiple_Messages_In_Parallel()
    {
        var queue = new InMemoryQueue(NullLoggerFactory.Instance.CreateLogger<InMemoryQueue>());
        const string topic = "concurrency-topic";
        var logger = NullLoggerFactory.Instance.CreateLogger("client");
        const int messages = 5;
        var processed = 0;
        var reset = new ManualResetEventSlim(false);

        InMemoryConsumerClient? client = null; // forward declaration for callback
        client = new InMemoryConsumerClient(logger, queue, "concurrent-group", 2)
        {
            OnMessageCallback = async (msg, _) =>
            {
                // Simulate small work
                await Task.Delay(50);
                Interlocked.Increment(ref processed);
                await client.CommitAsync(null); // release semaphore
                if (processed == messages) reset.Set();
            }
        };

        _ = client.SubscribeAsync([topic]);
        _ = Task.Run(() => client.ListeningAsync(TimeSpan.FromSeconds(10), default));

        for (var i = 0; i < messages; i++)
        {
            var headers = new Dictionary<string, string?>
            {
                [Headers.MessageId] = new SnowflakeId().NextId().ToString(),
                [Headers.MessageName] = topic
            };
            queue.Send(new TransportMessage(headers, Encoding.UTF8.GetBytes($"payload-{i}")));
        }

        reset.Wait(TimeSpan.FromSeconds(5));
        Assert.Equal(messages, processed);
    }
}
