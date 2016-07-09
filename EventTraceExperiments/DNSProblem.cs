using Microsoft.Diagnostics.Tracing;
using Microsoft.Diagnostics.Tracing.Parsers;
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
    class DNSProblem
    {
        public void NonResolvableDNS()
        {

            using (var session = new TraceEventSession("DNS"))
            {

                var dns = new MicrosoftWindowsDNSClientTraceEventParser(session.Source);
                session.EnableProvider(MicrosoftWindowsDNSClientTraceEventParser.ProviderGuid);
                //                session.EnableKernelProvider(KernelTraceEventParser.Keywords.Process);

                var processName = string.Empty;
                dns.All += (TraceEvent data) => Console.WriteLine(data.ToXml(new StringBuilder()));
                dns.task_03006 += (task_03006Args data) =>
                {
                    try
                    {
                        processName = data.ProcessName == string.Empty ? Process.GetProcessById(data.ProcessID).ProcessName : data.ProcessName;

                        Console.WriteLine(processName);
                        //if (processName == "ClientApp.vshost")
                        Console.WriteLine(data.Dump());
                        //Console.WriteLine(data.ToXml(new StringBuilder()));
                    }
                    catch (ArgumentException e)
                    {
                        //process is dead
                    }
                };


                // Set up Ctrl-C to stop the session
                Console.CancelKeyPress += (object s, ConsoleCancelEventArgs args) => session.Stop();

                // Subscribe to a callback that prints the information we wish 
              
                // Turn on the process events (includes starts and stops).  

                session.Source.Process();   // Listen (forever) for events
            }

        }

    }
}
