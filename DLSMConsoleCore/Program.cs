﻿using Gurux.Common;
using Gurux.DLMS;
using Gurux.DLMS.Enums;
using Gurux.DLMS.Objects;
using Gurux.DLMS.Secure;
using Gurux.Serial;
using System;
using System.IO.Ports;
using System.Linq;
using System.Text;

namespace DLSMConsoleCore
{
    class Program
    { 
        static GXDLMSSecureClient client = new GXDLMSSecureClient(true);
        static IGXMedia Media = null;
        
        private static int WaitTime = 500;

        static IGXMedia CreateComPortMedia(string portName = null)
        {

            GXSerial gXSerial = new  GXSerial();
            gXSerial.PortName = string.IsNullOrEmpty(portName) ? "COM1" : portName;
            gXSerial.BaudRate = 9600;
            gXSerial.DataBits = 8;
            gXSerial.Parity = Parity.None;
            gXSerial.StopBits = StopBits.One;
            return gXSerial;
        }
        static void Main(string[] args)
        {
           
            
            // Is used Logican Name or Short Name referencing.
            client.UseLogicalNameReferencing = true;

            // Is used HDLC or COSEM transport layers for IPv4 networks (WRAPPER)
            client.InterfaceType = InterfaceType.HDLC;

            // Read http://www.gurux.org/dlmsAddress
            // to find out how Client and Server addresses are counted.
            // Some manufacturers might use own Server and Client addresses.

            client.ClientAddress = 3;
            client.ServerAddress = GXDLMSClient.GetServerAddress(int.Parse("1"), 1339); 
            client.Password = Encoding.ASCII.GetBytes("00000003");
            client.Authentication = Authentication.Low;


            Media =  CreateComPortMedia("COM4");

            try
            {
                Media.Open();
            }
            catch (System.IO.IOException ex)
            {
                Console.WriteLine("----------------------------------------------------------");
                Console.WriteLine(ex.Message);
                Console.WriteLine("Available ports:");
                Console.WriteLine(string.Join(" ", GXSerial.GetPortNames()));
                return;
            }

            GXReplyData reply = new GXReplyData();
            //GXDLMSClient client = new GXDLMSClient();
            try
            {

                byte[] data;
                data = client.SNRMRequest();
                if (data != null)
                {
                    ReadDLMSPacket(data, reply);
                    //Has server accepted client.
                    client.ParseUAResponse(reply.Data);
                    Console.WriteLine("Parsing UA reply succeeded.");
                }

                //Generate AARQ request.
                //Split requests to multiple packets if needed. 
                //If password is used all data might not fit to one packet.
                foreach (byte[] it in client.AARQRequest())
                {
                    reply.Clear();
                    ReadDLMSPacket(it, reply);
                }
                //Parse reply.
                client.ParseAAREResponse(reply.Data);
                Console.WriteLine("Connection succeeded.");

                Console.WriteLine("starting to read Association Views...");

                /// Read Association View from the meter.
                reply = new GXReplyData();
                ReadDataBlock(client.GetObjectsRequest(), reply);
                GXDLMSObjectCollection objects = client.ParseObjects(reply.Data, true);

                Console.WriteLine($"Recieved: {objects.Count} objects");


                Console.WriteLine("now once done, disconnect");
                reply = new GXReplyData();
                ReadDLMSPacket(client.DisconnectRequest(), reply);
                Media.Close();
            }
            catch (Exception ex)
            {

                Console.WriteLine($"Exception: {ex}");
            }

            Console.WriteLine("finished");
            Console.ReadKey();
        }

        private static void ReadDataBlock(byte[] data, GXReplyData reply)
        {
            ReadDLMSPacket(data, reply);
            lock (Media.Synchronous)
            {
                while (reply.IsMoreData)
                {
                    if (reply.IsStreaming())
                    {
                        data = null;
                    }
                    else
                    {
                        data = client.ReceiverReady(reply);
                    }
                    ReadDLMSPacket(data, reply);
                        //If data block is read.
                    if ((reply.MoreData & RequestTypes.Frame) == 0)
                    {
                        Console.Write("+");
                    }
                    else
                    {
                        Console.Write("-");
                    }
                }
            }
        }
        /// <summary>
        /// Read DLMS Data from the device.
        /// </summary>
        /// <param name="data">Data to send.</param>
        /// <returns>Received data.</returns>
        private static void ReadDLMSPacket(byte[] data, GXReplyData reply)
        {
            if (data == null)
            {
                return;
            }
            object eop = (byte)0x7E;
            //In network connection terminator is not used.
            if (client.InterfaceType == InterfaceType.WRAPPER && (Media  is GXSerial) == false)
            {
                eop = null;
            }
            int pos = 0;
            bool succeeded = false;
            ReceiveParameters<byte[]> p = new ReceiveParameters<byte[]>()
            {
                AllData = true,
                Eop = eop,
                Count = 5,
                WaitTime = WaitTime,
            };
            lock (Media.Synchronous)
            {
                while (!succeeded && pos != 3)
                {
                    WriteTrace("<- " + DateTime.Now.ToLongTimeString() + "\t" + GXCommon.ToHex(data, true));

                    Media.Send(data, null);
                    succeeded = Media.Receive(p);
                    if (!succeeded)
                    {
                        //If Eop is not set read one byte at time.
                        if (p.Eop == null)
                        {
                            p.Count = 1;
                        }
                        //Try to read again...
                        if (++pos != 3)
                        {
                            System.Diagnostics.Debug.WriteLine("Data send failed. Try to resend " + pos.ToString() + "/3");
                            continue;
                        }
                        throw new Exception("Failed to receive reply from the device in given time.");
                    }
                }
                //Loop until whole COSEM packet is received.                
                while (!client.GetData(p.Reply, reply))
                {
                    //If Eop is not set read one byte at time.
                    if (p.Eop == null)
                    {
                        p.Count = 1;
                    }
                    if (!Media.Receive(p))
                    {
                        //Try to read again...
                        if (pos != 3)
                        {
                            System.Diagnostics.Debug.WriteLine("Data send failed. Try to resend " + pos.ToString() + "/3");
                            continue;
                        }
                        throw new Exception("Failed to receive reply from the device in given time.");
                    }
                }
            }
            WriteTrace("-> " + DateTime.Now.ToLongTimeString() + "\t" + GXCommon.ToHex(p.Reply, true));
            if (reply.Error != 0)
            {
                throw new GXDLMSException(reply.Error);
            }
        }

        private static void WriteTrace(string v)
        {
            Console.WriteLine($"trace:  ${v}");
        }
    }
}
