using System.Text.Json;
using DotNetCore.CAP;
using Microsoft.AspNetCore.Mvc;

namespace InMemorySample.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class HomeController : ControllerBase
    {
        private readonly ICapPublisher _cap;

        public HomeController(ICapPublisher cap)
        {
            _cap = cap;
        }

        [HttpGet("[Action]")]
        public async Task PublishAsync()
        {
            await _cap.PublishAsync("inmemory.test", new
            {
                Id = Guid.NewGuid(),
                Time = DateTime.Now
            });
        }

        [HttpGet("[Action]")]
        public async Task PublishDelayAsync()
        {
            await _cap.PublishDelayAsync(TimeSpan.FromSeconds(10), "inmemory.test", new
            {
                Id = Guid.NewGuid(),
                Time = DateTime.Now
            });
        }

        [CapSubscribe("inmemory.test", Group = "Hello")]
        [NonAction]
        public async Task SubscriberTest(JsonElement jEle, CancellationToken token)
        {
            Console.WriteLine($"-----------Hello Group----------------" + DateTime.Now);
            await Task.Delay(4000, token);
        }

        [CapSubscribe("inmemory.test", GroupConcurrent = 2)]
        [NonAction]
        public async Task SubscriberTestConcurrent(JsonElement jEle, CancellationToken token)
        {
            Console.WriteLine($"-------------SubscriberTestConcurrent----------------" + DateTime.Now);
            await Task.Delay(2000, token);
        }
    }
}