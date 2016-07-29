using Microsoft.Diagnostics.Tracing;
using Microsoft.Diagnostics.Tracing.Parsers;
using Microsoft.Diagnostics.Tracing.Parsers.Clr;
using Microsoft.Diagnostics.Tracing.Parsers.Kernel;
using Microsoft.Diagnostics.Tracing.Session;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Servicedog
{
    static class TCP
    {

        /// <summary>
        /// Capture a TCP reconnect event wich indicates an unreachable destination, just like a service down or a firewall blocking the way.
        /// </summary>
        public static void Reconnect()
        {

            using (var session = new TraceEventSession("servicedog-tcp-reconnect"))//TODO: define const
            {

                session.EnableKernelProvider(KernelTraceEventParser.Keywords.NetworkTCPIP, KernelTraceEventParser.Keywords.NetworkTCPIP);


                var proccessInfo = string.Empty;
                session.Source.Kernel.TcpIpReconnect+= (TcpIpTraceData data) =>
                {
                    try
                    {
                        if (string.IsNullOrEmpty(data.ProcessName))
                            proccessInfo = Process.GetProcessById(data.ProcessID).ProcessName;

                            Console.WriteLine("IPV4 " + proccessInfo + " tried reconnect on " + data.daddr + ":" +data.dport);
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

     

    }
}
