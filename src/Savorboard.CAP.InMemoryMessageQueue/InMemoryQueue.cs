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
        private static readonly object Lock = new object();

        internal readonly Dictionary<string, (Action<TransportMessage>, List<string>)> _groupTopics;

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
            lock (Lock)
            {
                _groupTopics.Clear();
            }
        }

        public void ClearSubscriber(string groupId)
        {
            lock (Lock)
            {
                _groupTopics.Remove(groupId);
            }
        }

        public void Send(TransportMessage message)
        {
            var name = message.GetName();
            foreach (var groupTopic in _groupTopics.Where(o => o.Value.Item2.Contains(name)))
            {
                try
                {
                    var message_copy = new TransportMessage(message.Headers.ToDictionary(o => o.Key, o => o.Value), message.Body);
                    message_copy.Headers[Headers.Group] = groupTopic.Key;
                    groupTopic.Value.Item1?.Invoke(message_copy);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, $"Consumption message raises an exception. Group-->{groupTopic.Key} Name-->{name}");
                }
            }
        }
    }
}
