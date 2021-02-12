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
                .WriteTo.File("Log.txt")
                .MinimumLevel.Debug()
                .CreateLogger();
            GXDLMSSecureClient client = new GXDLMSSecureClient(true);
            IGXMedia media = null;
            DLMSReader reader = null;
            string outputFile = "output.xml";

            try
            {
                Log.Logger.Information("Strating...");

                media = CreateComPortMedia("COM5");

                Log.Logger.Information("Connected to port COM5");

                client.UseLogicalNameReferencing = true;
                client.InterfaceType = InterfaceType.HDLC;
                client.ClientAddress = 3;
                client.ServerAddress = GXDLMSClient.GetServerAddress(int.Parse("1"), 1339);
                client.Password = Encoding.ASCII.GetBytes("00000003");
                client.Authentication = Authentication.Low;

                reader = new DLMSReader(Log.Logger, client, media);

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
            Log.Logger.Information("**********  Start reading now ********");
                var fileName = System.IO.Directory.GetCurrentDirectory() + "\\" + outputFile;
                
                if (System.IO.File.Exists(fileName))
                {
                    bool read = false;
                    if (outputFile != null)
                    {
                        Log.Logger.Information($"**********  reading from file: {fileName}");
                        try
                        {
                            client.Objects.Clear();
                            client.Objects.AddRange(GXDLMSObjectCollection.Load(fileName));
                            read = true;
                        }
                        catch (Exception ex)
                        {
                            //It's OK if this fails.
                        }
                    }
                    reader.InitializeConnection();

                    if (!read)
                    {
                        reader.GetAssociationView(null);
                    }
                    //foreach (KeyValuePair<string, int> it in readObjects)
                    //{
                    //    var obj = client.Objects.FindByLN(ObjectType.None, it.Key);
                    //    Log.Logger.Information($"reading: {obj.ToString()}");
                    //    object val = reader.Read(client.Objects.FindByLN(ObjectType.None, it.Key), it.Value);
                    //    //reader.ShowValue(val, it.Value);
                    //    Log.Logger.Information(val.ToString());
                    //}


                    //if (outputFile != null)
                    //{
                    //    try
                    //    {
                    //        client.Objects.Save(outputFile, new GXXmlWriterSettings() { UseMeterTime = true, IgnoreDefaultValues = false });
                    //    }
                    //    catch (Exception ex)
                    //    {
                    //        //It's OK if this fails.
                    //    }
                    //}
                }
                else
                {
                    Log.Logger.Information("************    Reading All    *********");
                    reader.ReadAll(outputFile);
                }
                Log.Logger.Information("************    Done reading    *********");


                do
                {
                    try
                    {

                    Log.Logger.Information("please enter the object [key:index] to read or x to exit");
                    var objectToRead = Console.ReadLine();
                    KeyValuePair<string, int> pair = new KeyValuePair<string, int>("", 0);
                    if (objectToRead == "x")
                        return;

                    if (objectToRead.Contains(":"))
                        pair = new KeyValuePair<string, int>(objectToRead.Split(":")[0], int.Parse(objectToRead.Split(":")[1]));
                    else
                        pair = new KeyValuePair<string, int>(objectToRead, 1);

                    //object val = reader.Read(client.Objects.FindByLN(ObjectType.None, pair.Key), pair.Value);
                    var obj = client.Objects.FindByLN(ObjectType.None, pair.Key);
                        //obj = client.Objects.FindByLN(ObjectType.Clock, "");
                        if (obj == null)
                            Log.Logger.Information("not found");
                        else
                        {
                            var msg = obj.ToString();

                            Log.Logger.Information(reader.PrintObject(obj));
                        }
                    //object val = reader.Read(obj, pair.Value);
                    //reader.ShowValue(val, pair.Value);
                    }
                    catch (Exception)
                    {
                        Log.Logger.Error("invalid input. try again");
                    }

                } while (true);
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
