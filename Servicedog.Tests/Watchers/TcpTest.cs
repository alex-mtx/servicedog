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
    public class TcpTest
    {
        /// <summary>
        /// this is a tricky test. the DNS must resolve the
        /// </summary>
        [Test(TestOf = typeof(TCP))]
        public void When_Tcp_Can_Not_Start_A_Conversation_Should_Notify_Via_Dispatcher()
        {
            // use always IP addresses or a resolvable DNS entry. 
            //Keep in mind that if the hostname could not be resolved, the first error will be a DNS, not a TCP.
            string falseDnsEntry = "http://127.0.0.1:59999"; 
            var cancel = new CancellationToken();

            var dispatcherMoq = new Mock<IDispatcher>();
            dispatcherMoq.Setup(x => x.Send(It.IsAny<int>(), It.IsAny<string>(), TCP.TCP_RECONNECT)).Verifiable();

            var tcp = new TCP(dispatcherMoq.Object);

            //act
            tcp.StartWatching(cancel);

            //now we force a tcp connection to nowhere
            Assert.Throws<AggregateException>(() => {
                using (var cli = new HttpClient())
                {
                    cli.BaseAddress = new Uri(falseDnsEntry);
                    cli.Timeout = new TimeSpan(0, 0, 1);
                    var asyncTask = cli.GetAsync(new Uri("/nothing", UriKind.Relative));
                    asyncTask.Wait();
                }
            });

            //give some room to ETW to raise the Tcp Event
           Thread.Sleep(3000);

            //did TCP send a message?
            Assert.DoesNotThrow(() => dispatcherMoq.Verify());
        }
    }
}
