using WebServer.database;
using WebServer.database.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebServer.MeterIntegration;

namespace WebServer.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MeterController : ControllerBase
    {
        private readonly ILogger<MeterController> _logger;
        private readonly DatabaseContextEF _databaseContext;

        public MeterController(ILogger<MeterController> logger, DatabaseContextEF databaseContext)
        {
            _logger = logger;
            _databaseContext = databaseContext;
        }

        [HttpGet]
        public IEnumerable<Meter> Get()
        {
            return _databaseContext.Meters.ToList();
        }

        [HttpGet("GetById/{id}")]
        public ActionResult<Meter> GetById([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var meter = _databaseContext.Meters.SingleOrDefault(s => s.Id == id);

            if (meter == null)
            {
                return NotFound();
            }
            return Ok(meter);
        }

        [HttpPost]
        public IActionResult Create([FromBody] Meter input)
        {
            if (input == null)
                throw new ArgumentNullException("input");
            _databaseContext.Meters.Add(input);
            _databaseContext.SaveChanges();

            return Ok(input);
        }
        [HttpPut]
        public IActionResult Update([FromBody] Meter input)
        {
            if (input == null)
                throw new ArgumentNullException("input");
            _databaseContext.Meters.Update(input);
            _databaseContext.SaveChanges();

            return Ok(input);
        }
    }
}
