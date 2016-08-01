using Microsoft.Diagnostics.Tracing;
using Microsoft.Diagnostics.Tracing.Session;
using System;
using System.Diagnostics;
using Servicedog.Utils;
using Servicedog.Manifests.Dns;
using Servicedog.Manifests.Dns.Win2008;
using Servicedog.Manifests.Dns.Win2012;

namespace Servicedog
{
    /// <summary>
    /// Below implementation depends on Operating System version, since provider has evolved between them.
    /// <see cref="https://technet.microsoft.com/en-us/library/dn305896(v=ws.11).aspx"/>
    /// </summary>
    public class DNS
    {
        public static void FailedQuery()
        {

            using (var session = new TraceEventSession("servicedog-dns"))//TODO: remove it
            {
                if (OSVersionChecker.IsWindows7Or2008R2())
                {
                    Console.WriteLine("DNSWin2008en");
                    var dns = new DnsWin2008Parser(session.Source);
                    session.EnableProvider(DnsWin2008Parser.ProviderName, providerLevel: TraceEventLevel.Informational,
                        matchAnyKeywords: 0x8000000000000000);

                    var proccessInfo = string.Empty;
                    dns.DnsNameError += (Manifests.Dns.Win2008.DnsAllServersTimeoutArgs data) =>
                    {
                        try
                        {
                            if(string.IsNullOrEmpty(data.ProcessName))
                                proccessInfo = Process.GetProcessById(data.ProcessID).ProcessName;

                            //filtering out our local domain
                            //if (data.QueryName.Contains("lanet"))
                              Console.WriteLine(proccessInfo + " failed on " + data.QueryName);
                        }
                        catch (ArgumentException)
                        {
                            //process is dead
                        }
                    };
                }
                else
                {
                    Console.WriteLine("DNSWin2012en");//TODO: remove it

                    var dns = new DnsWin2012Parser(session.Source);
                    session.EnableProvider(DnsWin2012Parser.ProviderGuid, TraceEventLevel.Informational,
                        matchAnyKeywords: 0x8000000000000000);

                    var proccessInfo = string.Empty;
                    dns.task_03009 += (Manifests.Dns.Win2012.task_03009Args data) =>
                     {
                         try
                         {
                             if (string.IsNullOrEmpty(data.ProcessName))
                                 proccessInfo = Process.GetProcessById(data.ProcessID).ProcessName;

                             //filtering out our local domain
                             //if (data.QueryName.Contains("lanet"))
                             Console.WriteLine(proccessInfo + " failed on " + data.QueryName);
                         }
                         catch (ArgumentException)
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

            using (var session = new TraceEventSession("servicedog-dns-dnsTimeout"))
            {

                if (OSVersionChecker.IsWindows7Or2008R2())
                {
                    Console.WriteLine("DNSWin2008en");
                    var dns = new DnsWin2008Parser(session.Source);
                    session.EnableProvider(DnsWin2008Parser.ProviderName, providerLevel: TraceEventLevel.Informational,
                        matchAnyKeywords: 0x8000000000000000);

                    var proccessInfo = string.Empty;
                    dns.DnsServerTimeout += (Manifests.Dns.Win2008.DnsAllServersTimeoutArgs data) =>
                    {
                        try
                        {
                            if (string.IsNullOrEmpty(data.ProcessName))
                                proccessInfo = Process.GetProcessById(data.ProcessID).ProcessName;

                            //filtering out our local domain
                            //if (data.QueryName.Contains("lanet"))
                            Console.WriteLine(proccessInfo + " DNS timed out " + data.QueryName);
                        }
                        catch (ArgumentException)
                        {
                            //process is dead
                        }
                    };
                }
                else
                {
                    Console.WriteLine("DNSWin2012en");

                    var dns = new DnsWin2012Parser(session.Source);
                    session.EnableProvider(DnsWin2012Parser.ProviderGuid, TraceEventLevel.Informational,
                        matchAnyKeywords: 0x8000000000000000 /* used by Windows Operational trace session in event viewer*/);

                    var proccessInfo = string.Empty;
                    dns.DnsServerTimeout += (Manifests.Dns.Win2012.DnsAllServersTimeoutArgs data) =>
                    {
                        try
                        {
                            if (string.IsNullOrEmpty(data.ProcessName))
                                proccessInfo = Process.GetProcessById(data.ProcessID).ProcessName;

                            //filtering out our local domain
                            //if (data.QueryName.Contains("lanet"))
                            Console.WriteLine(proccessInfo + "  DNS timed out  " + data.QueryName);
                        }
                        catch (ArgumentException)
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
    
    }
}
