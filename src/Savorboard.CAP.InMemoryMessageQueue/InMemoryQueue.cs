using System;
using System.Collections.Generic;
using System.Linq;
using DotNetCore.CAP.Messages;
using Microsoft.Extensions.Logging;

namespace Savorboard.CAP.InMemoryMessageQueue
{
    internal class InMemoryQueue
    {
        private readonly ILogger<InMemoryQueue> _logger;
        private static readonly object Lock = new();

        internal readonly Dictionary<string, (Action<TransportMessage>, List<string>)> GroupTopics;

        public InMemoryQueue(ILogger<InMemoryQueue> logger)
        {
            _logger = logger;
            GroupTopics = new Dictionary<string, (Action<TransportMessage>, List<string>)>();
        }

        public void Subscribe(string groupId, Action<TransportMessage> received, string topic)
        {
            lock (Lock)
            {
                if (GroupTopics.ContainsKey(groupId))
                {
                    var topics = GroupTopics[groupId];
                    if (!topics.Item2.Contains(topic))
                    {
                        topics.Item2.Add(topic);
                    }
                }
                else
                {
                    GroupTopics.Add(groupId, (received, new List<string> { topic }));
                }
            }
        }

        public void ClearSubscriber()
        {
            lock (Lock)
            {
                GroupTopics.Clear();
            }
        }

        public void ClearSubscriber(string groupId)
        {
            lock (Lock)
            {
                GroupTopics.Remove(groupId);
            }
        }

        public void Send(TransportMessage message)
        {
            var name = message.GetName();
            lock (Lock)
            {
                foreach (var groupTopic in GroupTopics.Where(o => o.Value.Item2.Contains(name)))
                {
                    try
                    {
                        var messageCopy = new TransportMessage(message.Headers.ToDictionary(o => o.Key, o => o.Value), message.Body)
                        {
                            Headers =
                            {
                                [Headers.Group] = groupTopic.Key
                            }
                        };
                        groupTopic.Value.Item1?.Invoke(messageCopy);
                    }
                    catch (Exception e)
                    {
                        _logger.LogError(e, $"Consumption message raises an exception. Group-->{groupTopic.Key} Name-->{name}");
                    }
                }
            }
        }
    }
}
