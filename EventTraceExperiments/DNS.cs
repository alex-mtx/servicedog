using Microsoft.Diagnostics.Tracing;
using Microsoft.Diagnostics.Tracing.Parsers;
using Microsoft.Diagnostics.Tracing.Parsers.DNS;
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
    /// <summary>
    /// Below implementation depends on Operating System version, since provider has evolved between them.
    /// <see cref="https://technet.microsoft.com/en-us/library/dn305896(v=ws.11).aspx"/>
    /// </summary>
    static class DNS
    {
        private static List<string> _ignoredTemplateTypes = new List<string>();


        public static void FailedQuery()
        {//task_03006Args

            using (var session = new TraceEventSession("servicedog-DNS-DnsNameError"))
            {
                if (OSVersionChecker.IsWindows7Or2008R2())
                {
                    Console.WriteLine("DNSWin2008en");
                    var dns = new DNSWin2008en(session.Source);
                    session.EnableProvider(DNSWin2008en.ProviderName, providerLevel: TraceEventLevel.Informational);

                    var proccessInfo = string.Empty;
                    dns.DnsNameError += (DnsAllServersTimeoutArgs data) =>
                    {
                        try
                        {
                            proccessInfo = Process.GetProcessById(data.ProcessID).ProcessName;

                            //filtering out our local domain
                            if (data.QueryName.Contains("lanet"))
                                Console.WriteLine(proccessInfo + " failed on " + data.QueryName);
                        }
                        catch (ArgumentException e)
                        {
                            //process is dead
                        }
                    };
                }
                else
                {
                    Console.WriteLine("DNSWin2012en");

                    var dns = new DNSWin2012en(session.Source);
                    session.EnableProvider(DNSWin2012en.ProviderGuid, TraceEventLevel.Informational,
                        matchAnyKeywords: (ulong)DNSWin2012en.Keywords.Utl2connectpath);

                    var proccessInfo = string.Empty;
                    dns.task_03006 += (task_03006Args data) =>
                     {
                         try
                         {
                             proccessInfo = Process.GetProcessById(data.ProcessID).ProcessName;

                            //filtering out our local domain
                            if (data.QueryName.Contains("lanet"))
                                 Console.WriteLine(proccessInfo + " failed on " + data.QueryName);
                         }
                         catch (ArgumentException e)
                         {
                            //process is dead
                        }
                     };
                }

                // Set up Ctrl-C to stop the session
                Console.CancelKeyPress += (object s, ConsoleCancelEventArgs args) => session.Stop();

                session.Source.Process();   // Listen (forever) for events
            }

        }

        /// <summary>
        /// Good to measure the amount of resolution failures over a specific address.
        /// Bad for determining long period errors like a non existent address e.g. google.comm. In this case, is it better to use <see cref="FailedQuery"/>
        /// </summary>
        public static void DNSTimeout()
        {//DnsAllServersTimeout

            using (var session = new TraceEventSession("DNSTimeout"))
            {

                var dns = new MicrosoftWindowsDNSClientTraceEventParser(session.Source);
                session.EnableProvider(MicrosoftWindowsDNSClientTraceEventParser.ProviderGuid);

                var processName = string.Empty;
                dns.DnsServerTimeout += (DnsAllServersTimeoutArgs data) =>
                {
                    try
                    {
                        processName = data.ProcessName == string.Empty ? Process.GetProcessById(data.ProcessID).ProcessName : data.ProcessName;

                        Console.WriteLine(processName + " DNSTimeout " + data.QueryName);
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
        public static void All()
        {
            SetupIgnoredEvents();
            using (var session = new TraceEventSession("DNS"))
            {

                var dns = new MicrosoftWindowsDNSClientTraceEventParser(session.Source);
                session.EnableProvider(MicrosoftWindowsDNSClientTraceEventParser.ProviderGuid);

                var processName = string.Empty;
                dns.All += (TraceEvent data) =>
                {
                    if (data.Dump().ContainsAny(_ignoredTemplateTypes))
                        return;

                    try
                    {
                        processName = data.ProcessName == string.Empty ? Process.GetProcessById(data.ProcessID).ProcessName : data.ProcessName;


                        Console.WriteLine(Environment.NewLine + processName + Environment.NewLine + data.Dump());
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
