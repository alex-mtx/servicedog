using Moq;
using NUnit.Framework;
using Servicedog.Messaging;
using Servicedog.Watchers;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;

namespace Servicedog.Tests.Watchers
{
    [TestFixture]
    public class DnsTest
    {

        [Test(TestOf = typeof(DnsWatcher))]
        public void When_A_Dns_Lookup_Times_Out_Should_Notify_Via_Dispatcher()
        {
            var cancel = new CancellationToken();

            var events = new List<Message>();

            var dispatcherMoq = WatcherTest.PrepareMock(events);

            // DnsWatcher.DNS_TIMED_OUT
            var dns = new DnsWatcher(dispatcherMoq.Object);

            //act
            dns.StartWatching(cancel);

            //now we force a DNS lookup to a non-existing DNS entry
            var falseDnsEntry = DateTime.Now.Ticks.ToString() + ".unit.test";
            CallService("http://" + falseDnsEntry);

            //give some room to ETW to raise the DNS error Event
            Thread.Sleep(2000);

            WatcherTest.AssertSendCalled(dispatcherMoq, Times.AtLeastOnce());
            WatcherTest.AssertExpectedEventSent(events, DnsWatcher.DNS_NAME_ERROR, falseDnsEntry);
        }

        private static void CallService(string unavailableServiceUrl)
        {
            try
            {

                using (var cli = new HttpClient())
                {
                    cli.BaseAddress = new Uri(unavailableServiceUrl);
                    cli.Timeout = new TimeSpan(0, 0, 1);
                    var asyncTask = cli.GetAsync(new Uri("/nothing", UriKind.Relative));
                    asyncTask.Wait();
                }
            }
            catch
            {
                //swallow it
            }
        }

    }
}
