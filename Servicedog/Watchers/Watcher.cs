using Microsoft.Diagnostics.Tracing.Session;
using Servicedog.Messaging;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Servicedog.Watchers
{
    public abstract class Watcher : IWatcher
    {
        protected IDispatcher _sender;
        protected const string SESSION_NAME_PREFIX = "servicedog-";

        public Watcher(IDispatcher sender)
        {
            _sender = sender;
        }

        /// <summary>
        /// Captures desired events and publish them in an async internal queue.
        /// <para>The trace session runs in another thread.</para>
        /// </summary>
        public void StartWatching(CancellationToken cancellation)
        {
            try
            {
                Task.Run(() =>
                    {
                        Debug.Assert(!string.IsNullOrEmpty(SessionName()));

                        using (var session = new TraceEventSession(SessionName()))
                        {
                            Capture(session);
                            cancellation.Register(() => session.Stop(true));
                            session.Source.Process();   // Listen (forever) for events
                        }
                    }, cancellation);
            }
            catch (TaskCanceledException)
            {
                //do nothing, just exit.
            }
            catch (Exception)
            {
                //TODO: log
                throw;
            }
        }
        protected abstract string SessionName();
        
        // Register your provider and subscribe to the events at your wish.
        //example:
        //session.EnableKernelProvider(KernelTraceEventParser.Keywords.NetworkTCPIP, KernelTraceEventParser.Keywords.NetworkTCPIP);
        //
        //session.Source.Kernel.TcpIpReconnect += (TcpIpTraceData data) =>
        //{
        //    try
        //    {
        //         _sender.Send(data.ProcessID + " "+ data.daddr + ":" + data.dport, "TcpIpReconnect");
        //     }
        //    catch(Exception e)
        //    {
        //         _log.Error(e);
        //        throw;
        //     }
        //};
        protected abstract void Capture(TraceEventSession session);

    }
}
