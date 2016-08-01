using Microsoft.Diagnostics.Tracing;
using Microsoft.Diagnostics.Tracing.Session;
using Servicedog.Manifests.Afd;
using System;
using System.Diagnostics;
using System.Text;
using System.Threading;

namespace Servicedog
{
    public class Winsock
    {
        public const string SessionName = "servicedog-winsock-afd";
        public const string ABORT = "winsock_abort";
        public const string CONNECT = "winsock_connect";
        public const string ERROR_ON_CONNECT = "winsock_error_on_connect";
        private IDispatcher _sender;

        public Winsock(IDispatcher sender)//TODO: get via DI, so we can have simpler impl and tests
        {
            _sender = new MessageDispatcher();
        }

        public void Capture(CancellationToken cancellation)
        {

            using (var session = new TraceEventSession(SessionName))
            {
                var parser = new WinsockAfdParser(session.Source);

                session.EnableProvider(WinsockAfdParser.ProviderName, /*TraceEventLevel.Error,*/  matchAnyKeywords: (ulong)0x8000000000000006);
                var proccessInfo = string.Empty;

                parser.AfdAbort += (AfdAbortAbortedArgs data) =>
                {
                    _sender.Send(data.ToXml(new StringBuilder()).ToString(), ABORT);
                };

                parser.AfdConnectWithAddress += (AfdConnectWithAddressConnectedArgs data) =>
                {
                    _sender.Send(data.ToXml(new StringBuilder()).ToString(), CONNECT);

                };

                parser.AfdConnect += (AfdCloseClosedArgs data) =>
                {
                    if (data.Level != TraceEventLevel.Error)
                        return;
                    try
                    {
                        _sender.Send(data.ToXml(new StringBuilder()).ToString(), ERROR_ON_CONNECT);
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

                cancellation.Register(() => session.Stop());

                session.Source.Process();   // Listen (forever) for events
            }

        }

    }
}
