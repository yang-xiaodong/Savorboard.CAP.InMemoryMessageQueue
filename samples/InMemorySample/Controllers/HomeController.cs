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

        [CapSubscribe("inmemory.test")]
        [NonAction]
        public async Task SubscriberTest(JsonElement jEle, CancellationToken token)
        {
            Console.WriteLine($"-------------{DateTime.Now}----------------");
            Console.WriteLine(jEle.ToString());
            await Task.Delay(100, token);
        }
    }
}