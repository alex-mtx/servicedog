using Servicedog.Analysers;
using Servicedog.Messaging.Dispatchers;
using Servicedog.Messaging.Receivers;
using Servicedog.Utils;
using Servicedog.Watchers;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Servicedog
{
    class ServiceRunner 
    {
        private readonly CancellationTokenSource _cancelTasks = new CancellationTokenSource();
        public ServiceRunner()
        {
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

        public bool Start()//TODO: need do simplify all these instaces...
        {
            var dispatcher = new MessageDispatcher();
            var processtable = new ProcessTable();

            var cancellation = _cancelTasks.Token;

            //TODO: need a better design here..
            new DNS(dispatcher)
                .StartWatching(cancellation);

            new TCP(dispatcher)
                .StartWatching(cancellation);

            new Process(dispatcher)
                .StartWatching(cancellation);

            //and here too
            new NetworkAnalyser(new MessageReceiver())
                .StartAnalysing(cancellation);

            return true;
        }

        public bool Stop()
        {
            _cancelTasks.Cancel();
            return true;
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


        public bool Shutdown()
        {
            _cancelTasks.Cancel();
            return true;

        }

    }
}
