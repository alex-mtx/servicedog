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
using System.Text;
using System.Threading.Tasks;

namespace Servicedog
{
    static class Winsock
    {

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


                parser.AfdConnect += (AfdCloseClosedArgs data) =>
                {
                    try
                    {
                        if (string.IsNullOrEmpty(data.ProcessName))
                            proccessInfo = Process.GetProcessById(data.ProcessID).ProcessName;
                        //TODO: get address in IP format: 
                        //http://stackoverflow.com/questions/1904160/getting-the-ip-address-of-a-remote-socket-endpoint
                        Console.WriteLine(data.Dump());
                            //Console.WriteLine("Winsock 11 " + proccessInfo + " failed on " + data.Dump + ":" +data.dport);
                    }
                    catch (ArgumentException)
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
