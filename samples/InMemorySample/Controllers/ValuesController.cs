using System;
using DotNetCore.CAP;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace InMemorySample.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        private readonly ILogger<ValuesController> _logger;
        private readonly ICapPublisher _capBus;

        public ValuesController(ILogger<ValuesController> logger, ICapPublisher capBus)
        {
            _logger = logger;
            _capBus = capBus;
        }

        // GET api/values
        [HttpGet("~/send")]
        public ActionResult Get()
        {
            _capBus.Publish("samples.time.show", DateTime.Now);

            return Ok();
        }

        [CapSubscribe("samples.time.show", Group = "v1")]
        public void ShowTime(DateTime time)
        {
            _logger.LogDebug("publisher sent time -->" + time);
        }

        [CapSubscribe("samples.time.show", Group = "v2")]
        public void ShowTime2(DateTime time2)
        {
            _logger.LogDebug("publisher sent time2 -->" + time2);
        }
    }
}
