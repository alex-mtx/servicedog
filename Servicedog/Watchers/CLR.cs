using Microsoft.Diagnostics.Tracing;
using Microsoft.Diagnostics.Tracing.Parsers;
using Microsoft.Diagnostics.Tracing.Parsers.Clr;
using Microsoft.Diagnostics.Tracing.Session;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Servicedog.Watchers
{
    static class CLR
    {

        /// <summary>
        /// Capture all raised exceptions.
        /// </summary>
        public static void ExceptionRaised()
        {

            using (var session = new TraceEventSession("servicedog-clr-exceptionStart"))//TODO: define const
            {

                session.EnableProvider(ClrTraceEventParser.ProviderGuid);


                var proccessInfo = string.Empty;
                session.Source.Clr.ExceptionStart += (ExceptionTraceData data) =>
                {
                    try
                    {
                        if (string.IsNullOrEmpty(data.ProcessName))
                            proccessInfo = System.Diagnostics.Process.GetProcessById(data.ProcessID).ProcessName;

                        if (proccessInfo.Contains("ClientApp"))
                            Console.WriteLine(proccessInfo + " Exception Raised ");
                    }
                    catch (ArgumentException)                    {
                        //process is dead
                    }
                };


                // Set up Ctrl-C to stop the session
                Console.CancelKeyPress += (object s, ConsoleCancelEventArgs args) => session.Stop();

                session.Source.Process();   // Listen (forever) for events
            }

        }

        /// <summary>
        /// Captures filtered exceptions before client code acts.
        /// </summary>
        public static void ExceptionCatchStart()
        {

            using (var session = new TraceEventSession("servicedog-clr-exceptionCatchStart"))
            {
                session.EnableProvider(ClrTraceEventParser.ProviderGuid);


                var proccessInfo = string.Empty;
                session.Source.Clr.ExceptionCatchStart += (ExceptionHandlingTraceData data) =>
                {
                    try
                    {
                        if (string.IsNullOrEmpty(data.ProcessName))
                            proccessInfo = System.Diagnostics.Process.GetProcessById(data.ProcessID).ProcessName;

                        Console.WriteLine(proccessInfo + " ExceptionCatchStart ");
                    }
                    catch (ArgumentException)
                    {
                        //process is dead
                    }
                };


                // Set up Ctrl-C to stop the session
                Console.CancelKeyPress += (object s, ConsoleCancelEventArgs args) => session.Stop();

                session.Source.Process();   // Listen (forever) for events
            }

        }

    }
}
