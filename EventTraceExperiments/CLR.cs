using Microsoft.Diagnostics.Tracing;
using Microsoft.Diagnostics.Tracing.Parsers;
using Microsoft.Diagnostics.Tracing.Parsers.Clr;
using Microsoft.Diagnostics.Tracing.Parsers.MicrosoftWindowsDNSClient;
using Microsoft.Diagnostics.Tracing.Session;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventTraceExperiments
{
    static class CLR
    {
        private static List<string> _ignoredTemplateTypes = new List<string>();

       /// <summary>
       /// Capture all raised exceptions.
       /// </summary>
        public static void ExceptionRaised()
        { 

            using (var session = new TraceEventSession("CLR.ExceptionRaised"))
            {

                session.EnableProvider(ClrTraceEventParser.ProviderGuid);
                

                var processName = string.Empty;
                session.Source.Clr.ExceptionStart += (ExceptionTraceData data) =>
                {
                    try
                    {
                        processName = data.ProcessName == string.Empty ? Process.GetProcessById(data.ProcessID).ProcessName : data.ProcessName;

                        Console.WriteLine(processName + Environment.NewLine + data.Dump());
                    }
                    catch (ArgumentException e)
                    {
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

            using (var session = new TraceEventSession("CLR.ExceptionRaised"))
            {

                session.EnableProvider(ClrTraceEventParser.ProviderGuid);


                var processName = string.Empty;
                session.Source.Clr.ExceptionCatchStart += (ExceptionHandlingTraceData data) =>
                {
                    try
                    {
                        processName = data.ProcessName == string.Empty ? Process.GetProcessById(data.ProcessID).ProcessName : data.ProcessName;

                        Console.WriteLine(processName + Environment.NewLine + data.Dump(true));
                    }
                    catch (ArgumentException e)
                    {
                        //process is dead
                    }
                };


                // Set up Ctrl-C to stop the session
                Console.CancelKeyPress += (object s, ConsoleCancelEventArgs args) => session.Stop();

                session.Source.Process();   // Listen (forever) for events
            }

        }

        private static void SetupIgnoredEvents()
        {
            _ignoredTemplateTypes.Add("DnsServerForInterfaceArgs");
        }
    }
}
