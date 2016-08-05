using Servicedog.Messaging;
using Servicedog.Watchers;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Servicedog.Analysers
{
    public class NetworkAnalyser : Analyser
    {
        private readonly IDictionary<string, Message> _tcpEvents = new Dictionary<string, Message>();

        public NetworkAnalyser(IReceiver receiver) : base(receiver) { }

        public override void Analyse(Message message)
        {
            switch (message.RoutingKey) {

                case TCP.TCP_RECONNECT:
                    {
                        if (_tcpEvents.ContainsKey(message.ToString())) //HACK: use a computed hash instead?
                        {
                            //ok there is an error. the application could not connect to destination

                            //Send error down the pipe
                            Console.WriteLine(" TCP failed to connect to " + message.ToJson());

                            //no need to keep this anymore.
                            _tcpEvents.Remove(message.ToString());

                        }
                        
                        //on the first event take no further action
                        _tcpEvents.Add(message.ToString(), message);
                        break;
                    }
                default:
                    Debug.Fail("not prepared to analyse " + message.RoutingKey);
                    //TODO: log
                    break;

        }

            Console.WriteLine(message.ToJson());
        }

        public override IEnumerable<string> PrefixesToSubscribeTo()
        {
            return new[] {  TCP.TCP_RECONNECT,
                          //  Watchers.DNS.DNS_NAME_ERROR,
                          //  Watchers.DNS.DNS_TIMED_OUT
                            };

        }
    }
}
