using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebServer.database;
using WebServer.MeterIntegration;

namespace WebServer.Controllers
{
    [ApiController]
    [Route("[controller]")]

    public class MeterTestingController : Controller
    {
        private readonly ReadersManager readersManager;
        private readonly ILogger<MeterTestingController> logger;
        private readonly DatabaseContextEF databaseContext;

        public MeterTestingController(ReadersManager _readersManager, ILogger<MeterTestingController> _logger, DatabaseContextEF _databaseContext)
        {
            readersManager = _readersManager;
            logger = _logger;
            databaseContext = _databaseContext;
        }
       
        [HttpGet]
        public async Task<IActionResult> GetMeterIntialization(int meterId)
        {
            try
            {
                var meter = databaseContext.Meters.SingleOrDefault(s => s.Id == meterId);
                var port = databaseContext.ComPortMedias.SingleOrDefault(s => s.MeterId == meterId);
                var mappings = databaseContext.MeterMappings.Where(w => w.MeterId == meterId).ToList();

                if (meter == null)
                    return NotFound("Meter");
                if (port == null)
                    return NotFound("Port");
                if (mappings == null)
                    return NotFound("Meter mappings");

                await readersManager.InitializeComPortMeter(meter, port, mappings);
                return Ok();
            }
            catch (Exception ex)
            {
                return Json(ex);
            }
        }
    }
}
