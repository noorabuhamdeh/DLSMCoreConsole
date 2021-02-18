using WebServer.database;
using WebServer.database.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebServer.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MeterMappingController : ControllerBase
    {
        private readonly ILogger<MeterMappingController> _logger;
        private readonly DatabaseContextEF _databaseContext;

        public MeterMappingController(ILogger<MeterMappingController> logger, DatabaseContextEF databaseContext)
        {
            _logger = logger;
            _databaseContext = databaseContext;
        }

        [HttpGet]
        public IEnumerable<MeterMapping> Get()
        {
            return _databaseContext.MeterMappings.ToList();
        }
        [HttpGet("GetById/{id}")]
        public ActionResult<MeterMapping> GetById([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var mapping = _databaseContext.MeterMappings.FirstOrDefault(f => f.Id == id);

            if (mapping == null)
            {
                return NotFound();
            }
            return Ok(mapping);
        }
        [HttpGet("GetByMeterId/{id}")]
        public ActionResult<IEnumerable< MeterMapping>> GetByMeterId([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var mapping = _databaseContext.MeterMappings.Where(s => s.MeterId == id).ToList();

            if (mapping == null)
            {
                return NotFound();
            }
            return Ok(mapping);
        }

        [HttpPost]
        public ActionResult<MeterMapping> Create([FromBody] MeterMapping input)
        {
            if (input == null)
                throw new ArgumentNullException("input");
            _databaseContext.MeterMappings.Add(input);
            _databaseContext.SaveChanges();
            return Ok(input);
        }
        [HttpPut]
        public ActionResult<MeterMapping> Update([FromBody] MeterMapping input)
        {
            if (input == null)
                throw new ArgumentNullException("input");
            _databaseContext.MeterMappings.Update(input);
            _databaseContext.SaveChanges();
            return Ok(input);
        }
    }
}
