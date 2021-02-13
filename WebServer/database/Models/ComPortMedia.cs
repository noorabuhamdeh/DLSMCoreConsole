using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebServer.database.Models
{

    public class ComPortMedia
    {
        public int Id { get; set; }
        public int MeterId { get; set; }
        public string PortName { get; set; }
        public int BaudRate { get; set; }
        public int DataBits { get; set; }
        public int Parity { get; set; }
        public int StopBits { get; set; }
    }
}
