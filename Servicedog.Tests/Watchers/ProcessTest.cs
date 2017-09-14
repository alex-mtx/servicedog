using Moq;
using NUnit.Framework;
using Servicedog.Messaging;
using Servicedog.Watchers;
using System;
using System.Net.Http;
using System.Threading;

namespace Servicedog.Tests.Watchers
{
    [TestFixture]
    public class ProcessTest
    {
        /// <summary>
        /// this is a tricky test. the DNS must resolve the
        /// </summary>
        [Test(TestOf = typeof(Servicedog.Watchers.Process))]
        public void When_A_New_Proccess_Is_Created_By_SO_Should_Notify_Via_Dispatcher()
        {
            // use always IP addresses or a resolvable DNS entry. 
            //Keep in mind that if the hostname could not be resolved, the first error will be a DNS, not a TCP.
            string falseDnsEntry = "http://127.0.0.1:59999"; 
            var cancel = new CancellationToken();

            var dispatcherMoq = new Mock<IDispatcher>();
            //dispatcherMoq.Setup(x => x.Send(It.IsAny<int>(), It.IsAny<string>(), Process.PROCESS_CREATION)).Verifiable();
            dispatcherMoq.Setup(x => x.Send(It.IsAny<IMessage>(),It.Is<string>(y=>y.Equals(Process.PROCESS_CREATION)))).Verifiable();

            //var expectedRoutingKey = Process.PROCESS_CREATION.AsSource().OfLikeness<string>().CreateProxy();

            var processWatcher = new Servicedog.Watchers.Process(dispatcherMoq.Object);

            //act
            var p = new System.Diagnostics.Process();
            p.StartInfo = new System.Diagnostics.ProcessStartInfo("cmd.exe");
            //p.StartInfo.WorkingDirectory = @"C:\Program Files\Chrome";
            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.UseShellExecute = false;
            p.Start();
            p.Close();

            //give some room to ETW to raise the  Event
            Thread.Sleep(5000);

            //did TCP send a message?
            Assert.DoesNotThrow(() => dispatcherMoq.Verify(m => m.Send(It.IsAny<int>(), It.IsAny<string>(), It.Is<string>(x=>x.Equals(Servicedog.Watchers.Process.PROCESS_CREATION)))
            , Times.AtLeastOnce()));

        }
    }
}
