using Servicedog.Analysers;
using Servicedog.Messaging;
using Servicedog.Messaging.Dispatchers;
using Servicedog.Messaging.Receivers;
using Servicedog.OS;
using Servicedog.Watchers;
using System;
using System.Threading;
using System.Threading.Tasks;

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

        public bool Start()//TODO: need do simplify all these instaces...
        {

            var cancellation = _cancelTasks.Token;

            //TODO: need a better design here.. I would like just to implement the specialized classes
            // and.. bang! something outside it just bring it into life!
            new DNS(_dispatcher)
                .StartWatching(cancellation);

            new TCP(_dispatcher)
                .StartWatching(cancellation);

            new Process(_dispatcher)
                .StartWatching(cancellation);

            //and here too
            new NetworkAnalyser(_dispatcher)
                .StartAnalysing(cancellation);

            new ProcessAnalyser(_dispatcher)
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
