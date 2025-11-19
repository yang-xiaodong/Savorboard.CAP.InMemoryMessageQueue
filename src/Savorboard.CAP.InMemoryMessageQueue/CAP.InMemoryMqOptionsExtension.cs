using DotNetCore.CAP;
using DotNetCore.CAP.Transport;
using Microsoft.Extensions.DependencyInjection;

// ReSharper disable once CheckNamespace
namespace Savorboard.CAP.InMemoryMessageQueue;

/// <summary>
/// CAP options extension for configuring in-memory message queue services.
/// </summary>
internal sealed class InMemoryMqOptionsExtension : ICapOptionsExtension
{
    /// <summary>
    /// Adds in-memory message queue services to the dependency injection container.
    /// </summary>
    /// <param name="services">The service collection to add services to</param>
    public void AddServices(IServiceCollection services)
    {
        services.AddSingleton(new CapMessageQueueMakerService("InMemoryQueue"));
        services.AddSingleton<InMemoryQueue>();
        services.AddSingleton<IConsumerClientFactory, InMemoryConsumerClientFactory>();
        services.AddSingleton<ITransport, InMemoryMqTransport>();
    }
}