using System;
using System.Collections.Generic;
using DotNetCore.CAP.Messages;
using Microsoft.Extensions.Logging;

namespace Savorboard.CAP.InMemoryMessageQueue
{
    internal class InMemoryQueue
    {
        private readonly ILogger<InMemoryQueue> _logger;
        private static readonly object Lock = new object();

        private readonly Dictionary<string, (Action<TransportMessage>, List<string>)> _groupTopics;

        public InMemoryQueue(ILogger<InMemoryQueue> logger)
        {
            _logger = logger;
            _groupTopics = new Dictionary<string, (Action<TransportMessage>, List<string>)>();
        }

        public void Subscribe(string groupId, Action<TransportMessage> received, string topic)
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

        public void Send(TransportMessage message)
        {
            foreach (var groupTopic in _groupTopics)
            {
                if (groupTopic.Value.Item2.Contains(message.GetName()))
                {
                    try
                    {
                        message.Headers[Headers.Group] = groupTopic.Key;
                        groupTopic.Value.Item1?.Invoke(message);
                    }
                    catch (Exception e)
                    {
                        _logger.LogError(e, $"Consumption message raises an exception. Group-->{groupTopic.Key} Name-->{message.GetName()}");
                    }
                }
            }
        }
    }
}
