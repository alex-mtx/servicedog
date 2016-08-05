using Microsoft.Diagnostics.Tracing.Parsers;
using Microsoft.Diagnostics.Tracing.Parsers.Kernel;
using Microsoft.Diagnostics.Tracing.Session;
using Servicedog.Messaging;
using System;
using System.Threading;

namespace Servicedog.Watchers
{
    /// <summary>
    /// Captures TCP events from Kernel ETW.
    /// </summary>
    public class TCP : Watcher
    {
        public const string TCP_RECONNECT = "TcpIpReconnect";


        public TCP(IDispatcher sender) : base(sender) { }

        

        protected override string SessionName()
        {
            return SESSION_NAME_PREFIX + "kernel-tcp";
        }

        protected override void Capture(TraceEventSession session)
        {
            session.EnableKernelProvider(KernelTraceEventParser.Keywords.NetworkTCPIP, KernelTraceEventParser.Keywords.NetworkTCPIP);

            //two events in a row for same process and IP/port means failure. 
            //It could be a firewall intervention, silently droping packets to that destination
            //or the end service is down. 
            session.Source.Kernel.TcpIpReconnect += (TcpIpTraceData data) =>
            {
                try
                {
                    //should we create a model, json, schema,...? for now, let´s keep it as simple as possible.
                    _sender.Send(data.ProcessID, data.ProcessID + " " + data.daddr + ":" + data.dport, TCP_RECONNECT);
                }
                catch (Exception)
                {
                    //TODO: log it
                    //TODO: check all Exceptions that can be thrown by the _sender instance.
                    throw;
                }
            };
        }
    }
}
