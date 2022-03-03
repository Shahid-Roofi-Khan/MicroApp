using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;

namespace OrderProcessing.Product.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ElasticController : ControllerBase
    {

        private readonly ILogger<ElasticController> _logger;
        private readonly IWebHostEnvironment _env;

        public ElasticController(ILogger<ElasticController> logger, IWebHostEnvironment env)
        {
            _logger = logger;
            _env = env;
        }

        // GET: api/values
        [HttpGet]
        public int GetRandomvalue()
        {
            var random = new Random();
            var randomValue = random.Next(0, 100);
            _logger.LogInformation($"Random Value is {randomValue}");


            // ----------------------------------------------------------------------------------------------- Logging an Object
            var position = new { Latitude = 25, Longitude = 134 };
            var elapsedMs = 34;

            _logger.LogInformation("Processed {@Position} in {Elapsed:000} ms.", position, elapsedMs);
            
            return randomValue;
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public string ThrowErrorMessage(int id)
        {
            try
            {
                if (id <= 0)
                    throw new Exception($"id cannot be less than or equal to o. value passed is {id}");
                return id.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }
            return string.Empty;
        }

    }
}
