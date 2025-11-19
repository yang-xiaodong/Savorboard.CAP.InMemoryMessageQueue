using System.Text.Json;
using DotNetCore.CAP;
using Microsoft.AspNetCore.Mvc;

namespace InMemorySample.Controllers;

/// <summary>
/// Home controller for testing CAP with in-memory message queue.
/// </summary>
[ApiController]
[Route("[controller]")]
public class HomeController(ICapPublisher cap) : ControllerBase
{
    /// <summary>
    /// Publishes a test message immediately.
    /// </summary>
    [HttpGet("[Action]")]
    public async Task PublishAsync()
    {
        await cap.PublishAsync("inmemory.test", new
        {
            Id = Guid.NewGuid(),
            Time = DateTime.Now
        });
    }

    /// <summary>
    /// Publishes a test message with a 10-second delay.
    /// </summary>
    [HttpGet("[Action]")]
    public async Task PublishDelayAsync()
    {
        await cap.PublishDelayAsync(TimeSpan.FromSeconds(10), "inmemory.test", new
        {
            Id = Guid.NewGuid(),
            Time = DateTime.Now
        });
    }

    /// <summary>
    /// Subscriber for the "Hello" group.
    /// </summary>
    [CapSubscribe("inmemory.test", Group = "Hello")]
    [NonAction]
    public async Task SubscriberTest(JsonElement jEle, CancellationToken token)
    {
        Console.WriteLine($"-----------Hello Group----------------{DateTime.Now}");
        await Task.Delay(4000, token);
    }

    /// <summary>
    /// Subscriber with concurrent processing.
    /// </summary>
    [CapSubscribe("inmemory.test", GroupConcurrent = 2)]
    [NonAction]
    public async Task SubscriberTestConcurrent(JsonElement jEle, CancellationToken token)
    {
        Console.WriteLine($"-------------SubscriberTestConcurrent----------------{DateTime.Now}");
        await Task.Delay(2000, token);
    }
}