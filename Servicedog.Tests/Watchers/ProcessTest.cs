using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Servicedog.Watchers;
using Moq;
using Servicedog.Messaging;
using System.Threading;

namespace Servicedog.Tests.Watchers
{
    [TestFixture]
    public class ProcessTest
    {
        [Test(TestOf = typeof(DNS))]
        public void When_A_Dns_Lookup_Times_Out_Should_Notify_Via_Dispatcher()
        {
            string falseDnsEntry = DateTime.Now.Ticks.ToString() + ".unit.test";
            var cancel = new System.Threading.CancellationToken();

            var dispatcherMoq = new Mock<IDispatcher>();
            dispatcherMoq.Setup(x => x.Send(It.IsAny<int>(), It.IsAny<string>(), DNS.DNS_TIMED_OUT)).Verifiable();

            var dns = new DNS(dispatcherMoq.Object);

            //act
            dns.StartWatching(cancel);

            //now we force a DNS lookup to a non-existing DNS entry
            var proc = System.Diagnostics.Process.Start("ping", falseDnsEntry);
            proc.WaitForExit();

            //give some room to ETW to raise the DNS error Event
            Thread.Sleep(2000);

            //did DNS send a message?
            Assert.DoesNotThrow(() => dispatcherMoq.Verify());
        }
    }
}
