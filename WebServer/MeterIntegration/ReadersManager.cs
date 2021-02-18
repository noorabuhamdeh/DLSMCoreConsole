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
        private IDLMSReader physicalMeter;
        private IList<KeyValuePair<GXDLMSObject, int>> readlist;

        public ReadersManager(ILogger<ReadersManager> logger)
        {
            this.logger = logger;
            readlist = new List<KeyValuePair<GXDLMSObject, int>>();
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
        public object Read(MeterMapping mapping)
        {
            physicalMeter.OpenMedia();
            var obj = physicalMeter.Read(physicalMeter.InnerClient.Objects.FindByLN(ObjectType.None, mapping.OBIS_Code), mapping.ValueIndex);
            return physicalMeter.InnerClient.Objects.FindByLN(ObjectType.None, mapping.OBIS_Code) as GXDLMSObject;
        }
        public  bool InitializeComPortMeter(Meter meter, ComPortMedia comPort, IList<MeterMapping> mappings)
        {
            //Task.Run(() =>
            //{
            bool result = false;
                if (meter == null)
                    throw new ArgumentNullException("meter");
                if (comPort == null)
                    throw new ArgumentNullException("comPort");
                if (mappings == null)
                    throw new ArgumentNullException("mappings");

                GXDLMSSecureClient client = new GXDLMSSecureClient(true);
                IGXMedia media = null;

                media = CreateComPortMedia(comPort);
                client.UseLogicalNameReferencing = meter.UseLogicalNameReferencing;
                client.InterfaceType = (InterfaceType)meter.InterfaceType;
                client.ClientAddress = meter.ClientAddress;
                client.ServerAddress = GXDLMSClient.GetServerAddress(meter.LogicalServer, meter.PhysicalServer);
                client.Password = Encoding.ASCII.GetBytes(meter.Password);
                client.Authentication = (Authentication)meter.AuthenticationType;

                physicalMeter = new DLMSReader(meter.Id, meter.Name,  client, media);


            if (string.IsNullOrEmpty(meter.ObjectsXMLDocument))
            {
                meter.ObjectsXMLDocument = physicalMeter.GetAssociationViewsXml();
                result = true;
            }
            else
            {
                physicalMeter.OpenMedia();
                physicalMeter.LoadAssociationViewsFromXml(meter.ObjectsXMLDocument);
                physicalMeter.InitializeConnection();
            }
             
            //readlist = mappings
            //        .Select(s =>
            //        {
            //            var pair = new KeyValuePair<GXDLMSObject, int>(physicalMeter.InnerClient.Objects.FindByLN(ObjectType.None, s.OBIS_Code), s.ValueIndex);
            //            return pair;
            //        })
            //        .ToList();
                
            //    return meter;
            //});
            return result;
        }
    }
}
