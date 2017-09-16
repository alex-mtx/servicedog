using Microsoft.Diagnostics.Tracing;
using Microsoft.Diagnostics.Tracing.Session;
using System;
using System.Diagnostics;
using Servicedog.OS;
using Servicedog.Manifests.Dns;
using Servicedog.Manifests.Dns.Win2008;
using Servicedog.Manifests.Dns.Win2012;
using Servicedog.Messaging;

namespace Servicedog.Watchers
{
    /// <summary>
    /// Below implementation depends on Operating System version, since provider has evolved between them.
    /// <see cref="https://technet.microsoft.com/en-us/library/dn305896(v=ws.11).aspx"/>
    /// </summary>
    public class DnsWatcher : Watcher
    {
        public const string DNS_NAME_ERROR = "dns_name_error";
        public const string DNS_TIMED_OUT = "dns_timed_out";
        public DnsWatcher(IDispatcher sender) : base(sender) { }

        protected override string SessionName()
        {
            return SESSION_NAME_PREFIX + "dns";
        }

        protected override void Capture(TraceEventSession session)//TODO: needs refactoring. too much logic and code in the same place.
        {
            if (OSVersionChecker.IsWindows7Or2008R2())
            {
                var dns = new DnsWin2008Parser(session.Source);
                session.EnableProvider(DnsWin2008Parser.ProviderName, providerLevel: TraceEventLevel.Informational,
                    matchAnyKeywords: 0x8000000000000000);

                dns.DnsNameError += (Manifests.Dns.Win2008.DnsAllServersTimeoutArgs data) =>
                {
                    try
                    {
                        _sender.Send(data.ProcessID,data.QueryName, DNS_NAME_ERROR);
                    }
                    catch (ArgumentException)
                    {
                        //process is dead
                    }
                };

                dns.DnsServerTimeout += (Manifests.Dns.Win2008.DnsAllServersTimeoutArgs data) =>
                {
                    try
                    {
                        _sender.Send(data.ProcessID, data.QueryName, DNS_TIMED_OUT);
                    }
                    catch (ArgumentException)
                    {
                        //process is dead
                    }
                };
            }
            else
            {
                var dns = new DnsWin2012Parser(session.Source);
                session.EnableProvider(DnsWin2012Parser.ProviderGuid, TraceEventLevel.Informational,
                    matchAnyKeywords: 0x8000000000000000);

                dns.DnsNameError += (Manifests.Dns.Win2012.DnsAllServersTimeoutArgs data) =>
                {
                    try
                    {   //TODO: identify only errors. currently it is getting all name resolutions
                        _sender.Send(data.ProcessID, data.QueryName, DNS_NAME_ERROR);
                    }
                    catch (ArgumentException)
                    {
                        //process is dead
                    }
                };

                dns.DnsServerTimeout += (Manifests.Dns.Win2012.DnsAllServersTimeoutArgs data) =>
                {
                    try
                    {
                        _sender.Send(data.ProcessID, data.QueryName, DNS_TIMED_OUT);
                    }
                    catch (ArgumentException)
                    {
                        //process is dead
                    }
                };
            }
        }
    }
}
