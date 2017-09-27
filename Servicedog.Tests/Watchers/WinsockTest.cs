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
    public class WinsockTest
    {
        /// <summary>
        /// this is a tricky test. the DNS must resolve the
        /// </summary>
        [Test(TestOf = typeof(WinsockWatcher))]
        public void When_A_Socket_Can_Not_Connect_Should_Notify_Via_Dispatcher()
        {
            // use always IP addresses or a valid and resolvable DNS entry, otherwise the problem will be a DNS error instead a TCP one. 
            //Keep in mind that if the hostname could not be resolved, the first error will be a DNS, not a TCP.
            string unavailableService = "127.0.0.1:59999";

            string unavailableServiceUrl = "http://" + unavailableService; 
            var cancel = new CancellationToken();
            var events = new List<Message>();
            var dispatcherMoq = WatcherTest.PrepareMock(events);
                         
            var winsock = new WinsockWatcher(dispatcherMoq.Object);

            //act
            winsock.StartWatching(cancel);
            Thread.Sleep(2000);
            //now we force a tcp connection to nowhere
            CallService(unavailableServiceUrl);

            //give some room to ETW to raise the Tcp Event
           Thread.Sleep(4000);

            WatcherTest.AssertSendCalled(dispatcherMoq, Times.AtLeastOnce());
            WatcherTest.AssertExpectedEventSent(events, WinsockWatcher.ERROR_ON_CONNECT, string.Empty);
        }

        private static void CallService(string unavailableServiceUrl)
        {
            try
            {

                using (var cli = new HttpClient())
                {
                    cli.Timeout = new TimeSpan(0, 0, 1);
                    var asyncTask = cli.GetAsync(new Uri(unavailableServiceUrl));
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
