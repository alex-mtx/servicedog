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
    public class ClrTest
    {
        /// <summary>
        /// this is a tricky test. the DNS must resolve the
        /// </summary>
        [Test(TestOf = typeof(ClrWatcher))]
        public void When_App_Throws_Exception_Should_Notify_Via_Dispatcher()
        {
            // use always IP addresses or a valid and resolvable DNS entry, otherwise the problem will be a DNS error instead a TCP one. 
            //Keep in mind that if the hostname could not be resolved, the first error will be a DNS, not a TCP.
            string unavailableService = "127.0.0.1:59999";

            string unavailableServiceUrl = "http://" + unavailableService; 
            var cancel = new CancellationToken();
            var events = new List<Message>();
            var dispatcherMoq = WatcherTest.PrepareMock(events);
                         
            var clr = new ClrWatcher(dispatcherMoq.Object);

            //act
            clr.StartWatching(cancel);
            //give it some room to start receiveing ETW events
            Thread.Sleep(1000);
            //now we force an exception to be thrown
            ThrowAndCatch();

            //give some room to ETW to raise the Tcp Event
           Thread.Sleep(1000);

            WatcherTest.AssertSendCalled(dispatcherMoq, Times.AtLeastOnce());
            WatcherTest.AssertExpectedEventSent(events, ClrWatcher.EXCEPTION_START, typeof(ApplicationException).FullName);
        }

        [Test(TestOf = typeof(ClrWatcher))]
        public void When_App_Starts_Catching_Exception_Should_Notify_Via_Dispatcher()
        {
            // use always IP addresses or a valid and resolvable DNS entry, otherwise the problem will be a DNS error instead a TCP one. 
            //Keep in mind that if the hostname could not be resolved, the first error will be a DNS, not a TCP.
            string unavailableService = "127.0.0.1:59999";

            string unavailableServiceUrl = "http://" + unavailableService;
            var cancel = new CancellationToken();
            var events = new List<Message>();
            var dispatcherMoq = WatcherTest.PrepareMock(events);

            var clr = new ClrWatcher(dispatcherMoq.Object);

            //act
            clr.StartWatching(cancel);
            //give it some room to start receiveing ETW events
            Thread.Sleep(1000);
            //now we force an exception to be thrown
            ThrowAndCatch();

            //give some room to ETW to raise the Tcp Event
            Thread.Sleep(1000);

            WatcherTest.AssertSendCalled(dispatcherMoq, Times.AtLeastOnce());
            WatcherTest.AssertExpectedEventSent(events, ClrWatcher.EXCEPTION_CATCH_START, nameof(ThrowAndCatch));
        }

        private static void ThrowAndCatch()
        {
            try
            {
                throw new ApplicationException();
            }
            catch (ApplicationException e)
            {
            }
        }
    }
}
