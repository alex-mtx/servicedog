using Microsoft.Diagnostics.Tracing.Parsers;
using Microsoft.Diagnostics.Tracing.Parsers.Clr;
using Microsoft.Diagnostics.Tracing.Session;
using Servicedog.Messaging;
using System;

namespace Servicedog.Watchers
{
    public class ClrWatcher : Watcher
    {
        public const string EXCEPTION_START = "Exception/Start";
        public const string EXCEPTION_CATCH_START = "ExceptionCatch/Start";

        public ClrWatcher(IDispatcher sender) : base(sender)
        {
        }

        protected override void Capture(TraceEventSession session)
        {
            session.EnableProvider(ClrTraceEventParser.ProviderGuid);


            session.Source.Clr.ExceptionStart += (ExceptionTraceData data) =>
            {
                var processName = string.Empty;
                try
                {
                    processName = System.Diagnostics.Process.GetProcessById(data.ProcessID).ProcessName;
                }
                catch (ArgumentException)
                {
                    //process is dead
                }
                finally
                {
                    _sender.Send(data.ProcessID, processName + " " + data.ToString(), EXCEPTION_START);

                }
            };


            session.Source.Clr.ExceptionCatchStart += (ExceptionHandlingTraceData data) =>
            {
                var processName = string.Empty;

                try
                {
                    processName = System.Diagnostics.Process.GetProcessById(data.ProcessID).ProcessName;
                }
                catch (ArgumentException)
                {
                    //process is dead
                }
                finally
                {
                    _sender.Send(data.ProcessID, processName + " " + data.ToString(), EXCEPTION_CATCH_START);

                }
            };
        }
    }
}
