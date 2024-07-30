using System;
using System.Collections.Generic;
using System.Linq;
using DotNetCore.CAP.Messages;
using Microsoft.Extensions.Logging;

namespace Savorboard.CAP.InMemoryMessageQueue
{
    internal class InMemoryQueue(ILogger<InMemoryQueue> logger)
    {
        private static readonly object Lock = new();

        private readonly Dictionary<string, List<string>> _topicGroups = new();
        private readonly Dictionary<string, InMemoryConsumerClient> _consumerClients = new();

        public void RegisterConsumerClient(string groupId, InMemoryConsumerClient consumerClient)
        {
            lock (Lock)
            {
                _consumerClients[groupId] = consumerClient;
            }
        }

        public void Subscribe(string groupId, IEnumerable<string> topics)
        {
            lock (Lock)
            {
                foreach (var topic in topics)
                {
                    if (_topicGroups.TryGetValue(topic, out var value))
                    {
                        if (!value.Contains(groupId))
                        {
                            value.Add(groupId);
                        }
                    }
                    else
                    {
                        _topicGroups.Add(topic, [groupId]);
                    }    
                }
            }
        }

        public void Unsubscribe(string groupId)
        {
            _consumerClients.Remove(groupId);
            logger.LogInformation("Removed consumer client from InMemoryQueue! --> Group:"+ groupId);

        }

        public void Send(TransportMessage message)
        {
            var name = message.GetName();
            lock (Lock)
            {
                if (_topicGroups.TryGetValue(name, out var groupList))
                {
                    foreach (var groupId in groupList)
                    {
                        if (_consumerClients.TryGetValue(groupId, out var consumerClient))
                        {
                            var messageCopy =
                                new TransportMessage(message.Headers.ToDictionary(o => o.Key, o => o.Value),
                                    message.Body)
                                {
                                    Headers =
                                    {
                                        [Headers.Group] = groupId
                                    }
                                };

                            consumerClient.AddSubscribeMessage(messageCopy);
                        }
                    }
                }
                else
                {
                    throw new InvalidOperationException(
                        $"Cannot find the corresponding group for {name}. Have you subscribed?");
                }
            }
        }
    }
}
