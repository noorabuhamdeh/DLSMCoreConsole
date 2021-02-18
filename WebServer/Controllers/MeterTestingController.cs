using Gurux.DLMS.Objects;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebServer.database;
using WebServer.database.Models;
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
       
        [HttpGet("GetMeterIntialization/{meterId}")]
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

                if (readersManager.InitializeComPortMeter(meter, port, mappings))
                {
                    databaseContext.Meters.Update(meter);
                    await databaseContext.SaveChangesAsync();
                }
                return Ok();
            }
            catch (Exception ex)
            {
                return Json(ex.Message);
            }
        }
        [HttpGet("ReadValue/{mappingId}")]
        public async Task<IActionResult> ReadValue(int  mappingId)
        {
            try
            {
                var mappingValue = databaseContext.MeterMappings.SingleOrDefault(m => m.Id == mappingId);
                if (mappingValue == null)
                    return NotFound("mappingVaulue");
                var value = readersManager.Read(mappingValue)  as GXDLMSObject;
                var json = "{" + $"desc:{value.Description},value:{value.GetValues()[mappingValue.ValueIndex]}" + "}";
                return Json(json);
            }
            catch (Exception ex)
            {
                return Json(ex.Message);
            }
        }
    }
}
