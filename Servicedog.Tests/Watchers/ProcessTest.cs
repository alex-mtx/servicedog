using Moq;
using NUnit.Framework;
using Servicedog.Messaging;
using Servicedog.OS;
using Servicedog.Watchers;
using System.Collections.Generic;
using System.Threading;

namespace Servicedog.Tests.Watchers
{
    public class ProcessTest
    {
        [OneTimeSetUp]
        public void EnsureConfiguration()
        {
            EnvironmentChecker.EnsureIsAdministrator();
        }
        /// <summary>
        /// this is a tricky test. the DNS must resolve the
        /// </summary>
        [Test(TestOf = typeof(ProcessWatcher))]
        public void When_A_New_Proccess_Is_Created_By_SO_Should_Notify_Via_Dispatcher()
        {
            // use always IP addresses or a resolvable DNS entry. 
            //Keep in mind that if the hostname could not be resolved, the first error will be a DNS, not a TCP.
            var cancel = new CancellationToken();

            var events = new List<Message>();
            var dispatcherMoq = WatcherTest.PrepareMock(events);

            //dispatcherMoq.Setup(x => x.Send(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()))
            //    .Callback<int, string, string>((id, body, routingKey) =>
            //    {
            //        remember that the OS is running while we run test, so we may capture lots of events 
            //        not related with this test. That's why we need to hold a list of all captured events
            //        events.Add(new Message { ProcessId = id, Body = body, RoutingKey = routingKey });

            //    });

            var processWatcher = new ProcessWatcher(dispatcherMoq.Object);
            
            //act
            processWatcher.StartWatching(cancel);
            Thread.Sleep(2000);
            
            var p = new System.Diagnostics.Process();
            p.StartInfo = new System.Diagnostics.ProcessStartInfo("cmd.exe");
            p.StartInfo.WorkingDirectory = @"C:\windows\temp";
            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.UseShellExecute = false;
            p.Start();
            p.Kill();

            //give some room to ETW to raise the  Event
            Thread.Sleep(3000);

            WatcherTest.AssertSendCalled(dispatcherMoq, Times.AtLeastOnce());
            WatcherTest.AssertExpectedEventSent(events, ProcessWatcher.PROCESS_CREATION, "cmd.exe");
        }
    }
}
