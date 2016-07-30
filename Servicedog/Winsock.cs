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
using ZeroMQ;

namespace Servicedog
{
    static class Winsock
    {
        public static string SessionName = "servicedog-winsock-afd";
        public static string Connect = "winsock_connect";
        public static string ErrorOnConnect = "winsock_error_on_connect";

        /// <summary>
        /// Capture a TCP reconnect event wich indicates an unreachable destination, just like a service down or a firewall bloking the way.
        /// </summary>
        public static void Capture(ZContext ctx)
        {
            //Pubsub envelope publisher
            using (var publisher = new ZSocket(ctx, ZSocketType.PUB))
            {
                publisher.Linger = TimeSpan.Zero;
                publisher.Bind("inproc://events");


                using (var session = new TraceEventSession(SessionName))
                {
                    var parser = new WinsockAfdParser(session.Source);

                    session.EnableProvider(WinsockAfdParser.ProviderName, /*TraceEventLevel.Error,*/  matchAnyKeywords: (ulong)0x8000000000000006);
                    var proccessInfo = string.Empty;

                    ulong socketId;

                    parser.AfdConnectWithAddress += (AfdConnectWithAddressConnectedArgs data) =>
                    {
                        using (var message = new ZMessage())//TODO: reuse instance between events
                        {
                            var address = Encoding.UTF8.GetString(data.Address);

                            message.Add(new ZFrame(Connect)); //envelope
                            message.Add(new ZFrame(data.Endpoint));//correlation id
                            message.Add(new ZFrame(address)); //body

                            publisher.Send(message);
                        }
                    };

                    parser.AfdConnect += (AfdCloseClosedArgs data) =>
                    {
                        if (data.Level != TraceEventLevel.Error)
                            return;
                        try
                        {
                            if (string.IsNullOrEmpty(data.ProcessName))
                                proccessInfo = Process.GetProcessById(data.ProcessID).ProcessName;

                            using (var message = new ZMessage())//TODO: reuse instance between events
                            {

                                message.Add(new ZFrame(ErrorOnConnect)); //envelope
                                message.Add(new ZFrame(data.Endpoint)); //correlation id
                                message.Add(new ZFrame("connection error")); //correlation id


                                publisher.Send(message);
                            }

                        }
                        catch (ArgumentException e)
                        {
                            //process is dead
                        }
                        catch (Exception e)
                        {
                            //now something really bad went on
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

        private static void Parser_task_017(task_06Args data)
        {
            if (string.IsNullOrEmpty(data.ProcessName))
                //proccessInfo = Process.GetProcessById(data.ProcessID).ProcessName;

                Console.WriteLine(data.Dump());
        }
    }
}
