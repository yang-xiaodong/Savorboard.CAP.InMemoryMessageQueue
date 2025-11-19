using DotNetCore.CAP;

// ReSharper disable once CheckNamespace
namespace Savorboard.CAP.InMemoryMessageQueue;

/// <summary>
/// Extension methods for configuring CAP with In-Memory message queue.
/// </summary>
public static class CapOptionsExtensions
{
    /// <summary>
    /// Configuration to use In-Memory message queue in CAP.
    /// </summary>
    /// <param name="options">CAP configuration options</param>
    /// <returns>The CAP options for method chaining</returns>
    public static CapOptions UseInMemoryMessageQueue(this CapOptions options)
    {
        options.RegisterExtension(new InMemoryMqOptionsExtension());
        return options;
    }
}