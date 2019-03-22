// Copyright (c) .NET Core Community. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using DotNetCore.CAP;
using Microsoft.Extensions.DependencyInjection;

// ReSharper disable once CheckNamespace
namespace Savorboard.CAP.InMemoryMessageQueue
{
    internal sealed class InMemoryMqOptionsExtension : ICapOptionsExtension
    {

        public void AddServices(IServiceCollection services)
        {
            services.AddSingleton<CapMessageQueueMakerService>();

            services.AddSingleton<IConsumerClientFactory, InMemoryConsumerClientFactory>();
            services.AddSingleton<IPublishExecutor, InMemoryMqPublishMessageSender>();
            services.AddSingleton<IPublishMessageSender, InMemoryMqPublishMessageSender>();
        }
    }
}