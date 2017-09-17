using Microsoft.Diagnostics.Tracing;
using Microsoft.Diagnostics.Tracing.Session;
using Servicedog.Manifests.Afd;
using Servicedog.Messaging;
using System;
using System.Diagnostics;
using System.Net;
using System.Text;
using System.Threading;

namespace Servicedog.Watchers
{
    public class WinsockWatcher : Watcher
    {
        public const string ABORT = "winsock_abort";
        public const string CONNECT = "winsock_connect";
        public const string ERROR_ON_CONNECT = "winsock_error_on_connect";

        public WinsockWatcher(IDispatcher sender) : base(sender) { }

        protected override void Capture(TraceEventSession session)
        {
            var parser = new WinsockAfdParser(session.Source);

            session.EnableProvider(WinsockAfdParser.ProviderName, /*TraceEventLevel.Error,*/  matchAnyKeywords: (ulong)0x8000000000000006);
            
            parser.AfdAbort += (AfdAbortAbortedArgs data) =>
            {
                _sender.Send(data.ProcessID, data.ToXml(new StringBuilder()).ToString(), ABORT);
            };

            //TODO: decode the IP address from data.Address... I spended days trying without success.
            parser.AfdConnectWithAddress += (AfdConnectWithAddressConnectedArgs data) =>
            {
                _sender.Send(data.ProcessID, data.ToXml(new StringBuilder()).ToString(), CONNECT);
            };

            parser.AfdConnect += (AfdCloseClosedArgs data) =>
            {
                if (data.Level != TraceEventLevel.Error)
                    return;
                try
                {
                    _sender.Send(data.ProcessID, data.ToXml(new StringBuilder()).ToString(), ERROR_ON_CONNECT);
                }
                catch (Exception e)
                {
                    //now something really bad went on
                }
            };
        }
    }
}
