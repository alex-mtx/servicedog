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

        public bool Start()
        {
            var cancellation = _cancelTasks.Token;
            Task.Run(() => new Winsock(new MessageDispatcher()).Capture(cancellation), cancellation);

            Task.Run(() => new WinsockAnalyser(new MessageReceiver(new []{ Winsock.ERROR_ON_CONNECT, Winsock.CONNECT,Winsock.ABORT })).Analyse(cancellation));

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
