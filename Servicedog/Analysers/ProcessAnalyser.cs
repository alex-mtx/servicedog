using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Servicedog.Messaging;
using Servicedog.Watchers;

namespace Servicedog.Analysers
{
    class ProcessAnalyser : Analyser
    {
        public ProcessAnalyser(IReceiver receiver, IDispatcher dispatcher) : base(receiver, dispatcher){}
        public ProcessAnalyser(IDispatcher dispatcher) : base( dispatcher){ }

        public override void Analyse(Message message)
        {
            Console.WriteLine(message.Body);
        }

        public override IEnumerable<string> PrefixesToSubscribeTo()
        {
            return new[] { Process.PROCESS_CREATION, Process.PROCESS_END };
        }
    }
}
