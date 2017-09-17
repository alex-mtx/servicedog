using Microsoft.Diagnostics.Tracing.Etlx;
using Microsoft.Diagnostics.Tracing.Parsers;
using Microsoft.Diagnostics.Tracing.Parsers.Kernel;
using Microsoft.Diagnostics.Tracing.Session;
using Servicedog.Messaging;
using System;

namespace Servicedog.Watchers
{
    /// <summary>
    /// Captures TCP events from Kernel ETW.
    /// </summary>
    public class TcpWatcher : Watcher
    {
        public const string TCP_RECONNECT = "TcpIpReconnect";

        public TcpWatcher(IDispatcher sender) : base(sender) { }

        protected override void Capture(TraceEventSession session)
        {
            session.EnableKernelProvider(KernelTraceEventParser.Keywords.NetworkTCPIP, KernelTraceEventParser.Keywords.NetworkTCPIP);

            //TraceLogEventSource is required on win 7
            //see: https://github.com/Microsoft/dotnetsamples/blob/master/Microsoft.Diagnostics.Tracing/TraceEvent/TraceEvent/41_TraceLogMonitor.cs
            using (TraceLogEventSource traceLogSource = TraceLog.CreateFromTraceEventSession(session))
            {

                //two events in a row for same process and IP/port means failure. 
                //It could be a firewall intervention, silently droping packets to that destination
                //or the end service is down. 
                traceLogSource.Kernel.TcpIpReconnect += (TcpIpTraceData data) =>
                {
                    try
                    {
                        _sender.Send(data.ProcessID, data.daddr + ":" + data.dport, TCP_RECONNECT);
                    }
                    catch (Exception)
                    {
                    //TODO: log it
                    //TODO: check all Exceptions that can be thrown by the _sender instance.
                    throw;
                    }
                };
                traceLogSource.Process();
            }
        }
    }
}
