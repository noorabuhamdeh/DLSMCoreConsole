using Gurux.Common;
using Gurux.DLMS;
using Gurux.DLMS.Enums;
using Gurux.DLMS.Objects;
using Gurux.DLMS.Secure;
using Gurux.Serial;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebServer.database;
using WebServer.database.Models;
using Task = System.Threading.Tasks.Task;

namespace WebServer.MeterIntegration
{
    public class ReadersManager
    {
        private readonly ILogger<ReadersManager> logger;
        private readonly DatabaseContextEF databaseContext;
        private IList<IDLMSReader> meters;
        private Dictionary<int, IList<KeyValuePair<GXDLMSObject, int>>> readlist;

        public ReadersManager(ILogger<ReadersManager> logger)
        {
            this.logger = logger;
            meters = new List<IDLMSReader>();
            readlist = new Dictionary<int, IList<KeyValuePair<GXDLMSObject, int>>>();
        }
        public IDLMSReader this[int meterId]
        {
            get
            {
                return meters.Where(w => w.Id == meterId).SingleOrDefault();
            }
        }
        private IGXMedia CreateComPortMedia(ComPortMedia comPort)
        {

            GXSerial gXSerial = new GXSerial();
            gXSerial.PortName = comPort.PortName;
            gXSerial.BaudRate = comPort.BaudRate;
            gXSerial.DataBits = comPort.DataBits;
            gXSerial.Parity = (Parity)comPort.Parity;
            gXSerial.StopBits = (StopBits)comPort.StopBits;
            return gXSerial;
        }
        //public void InitializeMeter(string meterName, IList<MeterMapping> mappings)
        //{
        //    if (string.IsNullOrEmpty(meterName))
        //        throw new ArgumentNullException("meterName");
        //    if (mappings == null)
        //        throw new ArgumentNullException("mappings");

        //    var meter = this[meterName];
        //    if (meter == null)
        //        throw new InvalidOperationException("something went wrong");

        //}
        public async Task< Meter> InitializeComPortMeter(Meter meter, ComPortMedia comPort, IList<MeterMapping> mappings)
        {
            await Task.Run(() =>
            {


                if (meter == null)
                    throw new ArgumentNullException("meter");
                if (comPort == null)
                    throw new ArgumentNullException("comPort");
                if (mappings == null)
                    throw new ArgumentNullException("mappings");

                GXDLMSSecureClient client = new GXDLMSSecureClient(true);
                IGXMedia media = null;
                DLMSReader reader = null;

                media = CreateComPortMedia(comPort);
                client.UseLogicalNameReferencing = meter.UseLogicalNameReferencing;
                client.InterfaceType = (InterfaceType)meter.InterfaceType;
                client.ClientAddress = meter.ClientAddress;
                client.ServerAddress = GXDLMSClient.GetServerAddress(meter.LogicalServer, meter.PhysicalServer);
                client.Password = Encoding.ASCII.GetBytes(meter.Password);
                client.Authentication = (Authentication)meter.AuthenticationType;

                reader = new DLMSReader(meter.Id, meter.Name,  client, media);

                meters.Add(reader);

                if (string.IsNullOrEmpty(meter.ObjectsXMLDocument))
                    meter.ObjectsXMLDocument = this[meter.Id].GetAssociationViewsXml();
                else
                    this[meter.Id].LoadAssociationViewsFromXml(meter.ObjectsXMLDocument);

                readlist[meter.Id] = mappings
                    .Select(s =>
                    {
                        var pair = new KeyValuePair<GXDLMSObject, int>(this[meter.Id].InnerClient.Objects.FindByLN(ObjectType.None, s.OBIS_Code), 1);
                        return pair;
                    })
                    .ToList();
            });

            return meter;
        }
    }
}
