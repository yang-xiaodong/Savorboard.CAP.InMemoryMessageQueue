using System;
using System.Collections.Generic;
using DotNetCore.CAP;
using Microsoft.Extensions.Logging;

namespace Savorboard.CAP.InMemoryMessageQueue
{
    internal class InMemoryQueue
    {
        private readonly ILogger<InMemoryQueue> _logger;
        private static readonly object Lock = new object();

        private readonly Dictionary<string, (Action<MessageContext>, List<string>)> _groupTopics;

        public InMemoryQueue(ILogger<InMemoryQueue> logger)
        {
            _logger = logger;
            _groupTopics = new Dictionary<string, (Action<MessageContext>, List<string>)>();
        }

        public void Subscribe(string groupId, Action<MessageContext> received, string topic)
        {
            lock (Lock)
            {
                if (_groupTopics.ContainsKey(groupId))
                {
                    var topics = _groupTopics[groupId];
                    if (!topics.Item2.Contains(topic))
                    {
                        topics.Item2.Add(topic);
                    }
                }
                else
                {
                    _groupTopics.Add(groupId, (received, new List<string> { topic }));
                }
            }
        }

        public void ClearSubscriber()
        {
            _groupTopics.Clear();
        }

        public void Send(string topic, string content)
        {
            foreach (var groupTopic in _groupTopics)
            {
                if (groupTopic.Value.Item2.Contains(topic))
                {
                    try
                    {
                        groupTopic.Value.Item1?.Invoke(
                            new MessageContext
                            {
                                Group = groupTopic.Key,
                                Name = topic,
                                Content = content
                            });
                    }
                    catch (Exception e)
                    {
                        _logger.LogError(e, $"Consumption message raises an exception. Group-->{groupTopic.Key} Name-->{topic}");
                    }
                }
            }
        }
    }
}
