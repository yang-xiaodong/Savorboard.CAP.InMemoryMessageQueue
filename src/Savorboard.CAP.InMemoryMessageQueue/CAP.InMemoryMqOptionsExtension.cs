using DotNetCore.CAP;
using DotNetCore.CAP.Transport;
using Microsoft.Extensions.DependencyInjection;

// ReSharper disable once CheckNamespace
namespace Savorboard.CAP.InMemoryMessageQueue
{
    internal sealed class InMemoryMqOptionsExtension : ICapOptionsExtension
    {

        public void AddServices(IServiceCollection services)
        {
            services.AddSingleton<CapMessageQueueMakerService>();

            services.AddSingleton<InMemoryQueue>();
            services.AddSingleton<IConsumerClientFactory, InMemoryConsumerClientFactory>();
            services.AddSingleton<ITransport, InMemoryMqTransport>();
        }
    }
}