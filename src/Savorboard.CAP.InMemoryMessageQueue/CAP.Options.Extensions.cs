using DotNetCore.CAP;

// ReSharper disable once CheckNamespace
namespace Savorboard.CAP.InMemoryMessageQueue
{
    public static class CapOptionsExtensions
    {
        /// <summary>
        /// Configuration to use In-Memory message queue in CAP.
        /// </summary>
        /// <param name="options">CAP configuration options</param>
        public static CapOptions UseInMemoryMessageQueue(this CapOptions options)
        {
            options.RegisterExtension(new InMemoryMqOptionsExtension());
            return options;
        }
    }
}