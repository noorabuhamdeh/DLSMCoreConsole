using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebServer.database.Models
{
    public class MeterMapping
    {
        public int Id { get; set; }
        public int MeterId { get; set; }
        public string OBIS_Code { get; set; }
        public string Description { get; set; }
        public int ValueIndex { get; set; }
        public int MappedToAddress { get; set; }
        public string DataType { get; set; }
    }
}
