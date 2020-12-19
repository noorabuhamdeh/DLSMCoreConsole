using Gurux.Common;
using Gurux.DLMS;
using Gurux.DLMS.Enums;
using Gurux.DLMS.Objects;
using Gurux.DLMS.Secure;
using Gurux.Serial;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;

namespace DLSMConsoleCore
{
    class Program
    {

        private static int WaitTime = 500;
        private static List<KeyValuePair<string, int>> readObjects = new List<KeyValuePair<string, int>>();

        static IGXMedia CreateComPortMedia(string portName = null)
        {

            GXSerial gXSerial = new GXSerial();
            gXSerial.PortName = string.IsNullOrEmpty(portName) ? "COM1" : portName;
            gXSerial.BaudRate = 9600;
            gXSerial.DataBits = 8;
            gXSerial.Parity = Parity.None;
            gXSerial.StopBits = StopBits.One;
            return gXSerial;
        }
        static void Main(string[] args)
        {

            Log.Logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .CreateLogger();
            GXDLMSSecureClient client = new GXDLMSSecureClient(true);
            IGXMedia media = CreateComPortMedia("COM4");
            var reader = new DLMSReader(Log.Logger, client, media);

            try
            {
                client.UseLogicalNameReferencing = true;
                client.InterfaceType = InterfaceType.HDLC;
                client.ClientAddress = 3;
                client.ServerAddress = GXDLMSClient.GetServerAddress(int.Parse("1"), 1339);
                client.Password = Encoding.ASCII.GetBytes("00000003");
                client.Authentication = Authentication.Low;

                try
                {
                    media.Open();
                }
                catch (System.IO.IOException ex)
                {
                    Log.Logger.Error("----------------------------------------------------------");
                    Log.Logger.Error(ex.Message);
                    Log.Logger.Error("Available ports:");
                    Log.Logger.Error(string.Join(" ", GXSerial.GetPortNames()));
                    return;
                }
            //Some meters need a break here.
            Thread.Sleep(1000);
                if (readObjects.Count != 0)
                {
                    bool read = false;
                    //if (settings.outputFile != null)
                    //{
                    //    try
                    //    {
                    //        settings.client.Objects.Clear();
                    //        settings.client.Objects.AddRange(GXDLMSObjectCollection.Load(settings.outputFile));
                    //        read = true;
                    //    }
                    //    catch (Exception)
                    //    {
                    //        //It's OK if this fails.
                    //    }
                    //}
                    reader.InitializeConnection();

                    if (!read)
                    {
                        reader.GetAssociationView(null);
                    }
                    foreach (KeyValuePair<string, int> it in readObjects)
                    {
                        object val = reader.Read(client.Objects.FindByLN(ObjectType.None, it.Key), it.Value);
                        reader.ShowValue(val, it.Value);
                    }
                    //if (settings.outputFile != null)
                    //{
                    //    try
                    //    {
                    //        settings.client.Objects.Save(settings.outputFile, new GXXmlWriterSettings() { UseMeterTime = true, IgnoreDefaultValues = false });
                    //    }
                    //    catch (Exception)
                    //    {
                    //        //It's OK if this fails.
                    //    }
                    //}
                    //}
                    //else
                    //{
                    //    reader.ReadAll(settings.outputFile);
                    //}
                }
            }
            catch (GXDLMSException ex)
            {
                Log.Logger.Error(ex.Message);
            }
            catch (GXDLMSExceptionResponse ex)
            {
                Log.Logger.Error(ex.Message);
            }
            catch (GXDLMSConfirmedServiceError ex)
            {
                Log.Logger.Error(ex.Message);
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex.Message);
                Log.Logger.Error(ex.ToString());
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                }
                if (System.Diagnostics.Debugger.IsAttached)
                {
                    Log.Logger.Information("Ended. Press any key to continue.");
                }
            }
            Console.ReadKey();
        }
    }
}
