// Copyright (c) .NET Core Community. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using DotNetCore.CAP;
using Microsoft.Extensions.Logging;

namespace Savorboard.CAP.InMemoryMessageQueue
{
    internal sealed class InMemoryConsumerClientFactory : IConsumerClientFactory
    {
        private readonly ILoggerFactory _loggerFactory;

        public InMemoryConsumerClientFactory(ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory;
        }

        public IConsumerClient Create(string groupId)
        {
            var logger = _loggerFactory.CreateLogger(typeof(InMemoryConsumerClient));
            return new InMemoryConsumerClient(logger, groupId);
        }
    }
}