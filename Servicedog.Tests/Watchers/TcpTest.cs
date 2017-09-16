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
    public class TcpTest
    {
        /// <summary>
        /// this is a tricky test. the DNS must resolve the
        /// </summary>
        [Test(TestOf = typeof(TcpWatcher))]
        public void When_Tcp_Can_Not_Start_A_Conversation_Should_Notify_Via_Dispatcher()
        {
            // use always IP addresses or a resolvable DNS entry. 
            //Keep in mind that if the hostname could not be resolved, the first error will be a DNS, not a TCP.
            string unavailableService = "127.0.0.1:59999";

            string unavailableServiceUrl = "http://" + unavailableService; 
            var cancel = new CancellationToken();
            var events = new List<Message>();
            var dispatcherMoq = WatcherTest.PrepareMock(events);
                         
            var tcp = new TcpWatcher(dispatcherMoq.Object);

            //act
            tcp.StartWatching(cancel);
            Thread.Sleep(1000);
            //now we force a tcp connection to nowhere
            CallService(unavailableServiceUrl);

            //give some room to ETW to raise the Tcp Event
           Thread.Sleep(3000);

            WatcherTest.AssertMockCheckDoesNotThrow(dispatcherMoq, Times.AtLeastOnce());
            WatcherTest.AssertExpectedEventIsSent(events, TcpWatcher.TCP_RECONNECT, unavailableService);
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
