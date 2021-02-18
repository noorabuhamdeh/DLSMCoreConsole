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
    public class ComPortController : ControllerBase
    {
        private readonly ILogger<MeterMappingController> _logger;
        private readonly DatabaseContextEF _databaseContext;

        public ComPortController(ILogger<MeterMappingController> logger, DatabaseContextEF databaseContext)
        {
            _logger = logger;
            _databaseContext = databaseContext;
        }

        [HttpGet]
        public IEnumerable<ComPortMedia> Get()
        {
            return _databaseContext.ComPortMedias.ToList();
        }

        [HttpGet("GetById/{mappingId}")]
        public ActionResult<ComPortMedia> GetById([FromRoute] int mappingId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var mapping = _databaseContext.ComPortMedias.FirstOrDefault(f => f.Id == mappingId);

            if (mapping == null)
            {
                return NotFound();
            }
            return Ok(mapping);
        }

        [HttpGet("GetByMeterId/{id}")]
        public ActionResult<IEnumerable< ComPortMedia>> GetByMeterId([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var mapping = _databaseContext.ComPortMedias.Where(s => s.MeterId == id).ToList();

            if (mapping == null)
            {
                return NotFound();
            }
            return Ok(mapping);
        }

        [HttpPost]
        public ActionResult<ComPortMedia> Create([FromBody] ComPortMedia input)
        {
            if (input == null)
                throw new ArgumentNullException("input");
            _databaseContext.ComPortMedias.Add(input);
            _databaseContext.SaveChanges();
            return Ok(input);
        }
        [HttpPut]
        public ActionResult<ComPortMedia> Update([FromBody] ComPortMedia input)
        {
            if (input == null)
                throw new ArgumentNullException("input");
            _databaseContext.ComPortMedias.Update(input);
            _databaseContext.SaveChanges();
            return Ok(input);
        }
    }
}
