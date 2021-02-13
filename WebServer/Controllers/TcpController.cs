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
    public class TcpController : ControllerBase
    {
        private readonly ILogger<MeterMappingController> _logger;
        private readonly DatabaseContextEF _databaseContext;

        public TcpController(ILogger<MeterMappingController> logger, DatabaseContextEF databaseContext)
        {
            _logger = logger;
            _databaseContext = databaseContext;
        }

        [HttpGet]
        public IEnumerable<TcpMedia> Get()
        {
            return _databaseContext.TcpMedias.ToList();
        }
        [HttpGet("GetById/{id}")]
        public ActionResult<TcpMedia> GetById([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var mapping = _databaseContext.TcpMedias.FirstOrDefault(f => f.Id == id);

            if (mapping == null)
            {
                return NotFound();
            }
            return Ok(mapping);
        }
        [HttpGet("GetByMeterId/{id}")]
        public ActionResult<IEnumerable< TcpMedia>> GetByMeterId([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var mapping = _databaseContext.TcpMedias.Where(s => s.MeterId == id).ToList();

            if (mapping == null)
            {
                return NotFound();
            }
            return Ok(mapping);
        }

        [HttpPost]
        public ActionResult<TcpMedia> Create([FromBody] TcpMedia input)
        {
            if (input == null)
                throw new ArgumentNullException("input");
            _databaseContext.TcpMedias.Add(input);
            _databaseContext.SaveChanges();
            return Ok(input);
        }
        [HttpPut]
        public ActionResult<TcpMedia> Update([FromBody] TcpMedia input)
        {
            if (input == null)
                throw new ArgumentNullException("input");
            _databaseContext.TcpMedias.Update(input);
            _databaseContext.SaveChanges();
            return Ok(input);
        }
    }
}
