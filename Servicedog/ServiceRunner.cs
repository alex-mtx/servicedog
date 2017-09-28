using Servicedog.Analysers;
using Servicedog.Messaging;
using Servicedog.OS;
using Servicedog.Watchers;
using System;
using System.Threading;

namespace Servicedog
{
    public class ServiceRunner 
    {
        private readonly CancellationTokenSource _cancelTasks = new CancellationTokenSource();
        private readonly IProcessTable _processtable;
        private readonly IDispatcher _dispatcher;

 

        public ServiceRunner(IDispatcher messaging,IProcessTable processTable )
        {
            _dispatcher = messaging;
            _processtable = processTable;
#if DEBUG
            Start();
            Console.WriteLine("Ctrl+c to stop");
            Console.ReadKey();
            // Set up Ctrl-C to stop the sessions
            Console.CancelKeyPress += (object s, ConsoleCancelEventArgs args) => _cancelTasks.Cancel();
            Console.WriteLine("tasks cancelled.");
            Console.ReadKey();
#endif
        }

        public void Start()
        {

            var cancellation = _cancelTasks.Token;

            var watchers = Infrastructure.SetupApp.InstantiateWatchers(_dispatcher);
            watchers.ForEach(watcher => watcher.StartWatching(cancellation));

        }

        public void Stop()
        {
            _cancelTasks.Cancel();
        }

        //not yet necessary
        //public bool Pause()
        //{
        //    return true;

        //}

        //public bool Continue()
        //{
        //    return true;

        //}


        public void Shutdown()
        {
            _cancelTasks.Cancel();

        }

    }
}
