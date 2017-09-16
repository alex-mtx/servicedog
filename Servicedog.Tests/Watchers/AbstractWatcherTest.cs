using Moq;
using NUnit.Framework;
using Servicedog.Messaging;
using System.Collections.Generic;
using System.Linq;


namespace Servicedog.Tests.Watchers
{
    public class WatcherTest
    {

        public static void AssertExpectedEventIsSent(List<Message> events,string expectedRoutingKey, string partialBodyContents)
        {

            Assert.IsTrue(
                events.Exists(message => message.RoutingKey.Equals(expectedRoutingKey)
                                   && message.Body.Contains(partialBodyContents)));
        }

        public static void AssertMockCheckDoesNotThrow(Mock<IDispatcher> dispatcherMoq, Times expectedExecutions)
        {
            Assert.DoesNotThrow(() =>
             dispatcherMoq.Verify(
                 m => m.Send(
                     It.IsAny<int>(),
                     It.IsAny<string>(),
                     It.IsAny<string>()),
                 expectedExecutions)
             );
        }

        public static Mock<IDispatcher> PrepareMock(List<Message> events)
        {
            var dispatcherMoq = new Mock<IDispatcher>();

            dispatcherMoq.Setup(x => x.Send(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()))
                .Callback<int, string, string>((id, body, routingKey) =>
                {
                    //remember that the OS is running while we run test, so we may capture lots of events 
                    //not related with this test. That's why we need to hold a list of all captured events
                    events.Add(new Message { ProcessId = id, Body = body, RoutingKey = routingKey });

                });

            return dispatcherMoq;
        }
    }
}
