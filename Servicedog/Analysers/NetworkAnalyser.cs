using Servicedog.Messaging;
using System;
using System.Collections.Generic;

namespace Servicedog.Analysers
{
    public class NetworkAnalyser : Analyser
    {

        public NetworkAnalyser(IReceiver receiver) : base(receiver) { }

        public override void Analyse(Tuple<int,string, string> message)
        {
            Console.WriteLine(message.ToString());
        }

        public override IEnumerable<string> PrefixesToSubscribeTo()
        {
            return new[] {  Watchers.TCP.TCP_RECONNECT,
                            Watchers.DNS.DNS_NAME_ERROR,
                            Watchers.DNS.DNS_TIMED_OUT
                            };

        }
    }
}
