using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Servicedog.Watchers;
using Moq;
using Servicedog.Messaging;

namespace Servicedog.Tests.Watchers
{
    [TestFixture]
    public class ProcessTest
    {
        [Test(TestOf = typeof(DNS))]
        public void When_A_Dns_Entry_Is_Missing_Should_Notify_Via_Dispatcher()
        {
            var proc = System.Diagnostics.Process.Start("ping", DateTime.Now.Ticks.ToString() + ".unit.test");

            var dispatcherMoq = new Mock<IDispatcher>();
            dispatcherMoq.Setup(x => x.Send(proc.Id,"", DNS.DNS_TIMED_OUT)).Verifiable();
            var dns = new DNS(dispatcherMoq.Object);


            Assert.DoesNotThrow(() => dispatcherMoq.Verify());


        }
    }
}
