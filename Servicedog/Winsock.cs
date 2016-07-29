using Microsoft.Diagnostics.Tracing;
using Microsoft.Diagnostics.Tracing.Parsers;
using Microsoft.Diagnostics.Tracing.Parsers.Clr;
using Microsoft.Diagnostics.Tracing.Parsers.Kernel;
using Microsoft.Diagnostics.Tracing.Session;
using Servicedog.Manifests.Afd;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Servicedog
{
    static class Winsock
    {
        private static ZeroMQ.ZContext _messaging;

        public static void SetUp(ZeroMQ.ZContext messaging)
        {
            _messaging = messaging;
        }
        /// <summary>
        /// Capture a TCP reconnect event wich indicates an unreachable destination, just like a service down or a firewall bloking the way.
        /// </summary>
        public static void Failed()
        {

            using (var session = new TraceEventSession("servicedog-winsock-failed"))//TODO: define const
            {
                var parser = new WinsockAfdParser(session.Source);

                session.EnableProvider(WinsockAfdParser.ProviderName, TraceEventLevel.Error,  matchAnyKeywords:(ulong)0x8000000000000006);
                var proccessInfo = string.Empty;

                ulong socketId;
                byte[] address;

                //this is expected to happen before afdconnect
                parser.AfdConnectWithAddress += (AfdConnectWithAddressConnectedArgs data) =>
                {
                    socketId = data.Endpoint;
                    address = data.Address;
                };

                parser.AfdConnect += (AfdCloseClosedArgs data) =>
                {
                    if (data.Level != TraceEventLevel.Error)
                        return;
                    try
                    {
                        if (string.IsNullOrEmpty(data.ProcessName))
                            proccessInfo = Process.GetProcessById(data.ProcessID).ProcessName;
                        Console.WriteLine(data.Dump());

                    }
                    catch (ArgumentException e)
                    {
                        //process is dead
                    }
                };

                //TODO: verify whether ipv6 should be implemented
                //session.Source.Kernel.TcpIpReconnectIPV6 += (TcpIpV6TraceData data) =>
                //{
                //    try
                //    {
                //        if (string.IsNullOrEmpty(data.ProcessName))
                //            proccessInfo = Process.GetProcessById(data.ProcessID).ProcessName;

                //        Console.WriteLine("IPV6 " + proccessInfo + " tried reconnect on " + data.daddr + ":" + data.dport);
                //    }
                //    catch (ArgumentException)
                //    {
                //        //process is dead
                //    }
                //};


                // Set up Ctrl-C to stop the session
                Console.CancelKeyPress += (object s, ConsoleCancelEventArgs args) => session.Stop();

                session.Source.Process();   // Listen (forever) for events
            }

        }

        private static void Parser_task_017(task_06Args data)
        {
            if (string.IsNullOrEmpty(data.ProcessName))
                //proccessInfo = Process.GetProcessById(data.ProcessID).ProcessName;

            Console.WriteLine(data.Dump());
        }
    }
}
