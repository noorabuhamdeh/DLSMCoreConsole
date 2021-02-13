using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebServer.database.Models
{
    public class Meter
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ObjectsXMLDocument { get; set; }
        public int AuthenticationType { get; set; }
        public string Password { get; set; }
        public int PhysicalServer { get; set; }
        public int LogicalServer { get; set; }
        public int ClientAddress { get; set; }
        public string ManufactureName { get; set; }
        public bool UseLogicalNameReferencing { get; set; }
        public int InterfaceType { get; set; }
        public DateTime LastForcedReadTime { get; set; } = DateTime.Now;
    }
}
