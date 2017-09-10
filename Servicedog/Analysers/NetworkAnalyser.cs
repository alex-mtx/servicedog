using Servicedog.Messaging;
using Servicedog.Watchers;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Servicedog.Analysers
{
    /// <summary>
    /// Takes care of networking related issues/behaviors.
    /// </summary>
    public class NetworkAnalyser : Analyser
    {
        //these are the routing keys used to subscribe to each message channel on the base class
        public const string NETWORK_UNREACHABLE_DESTINATION = "NETWORK_UNREACHABLE_DESTINATION";
        public const string NETWORK_COULD_NOT_RESOLVE_NAME = "NETWORK_COULD_NOT_RESOLVE_NAME";
        public const string NETWORK_DNS_QUERY_TIMEOUT = "NETWORK_DNS_QUERY_TIMEOUT";

        private readonly IDictionary<string, Message> _tcpEvents = new Dictionary<string, Message>();

        public NetworkAnalyser(IReceiver receiver, IDispatcher dispatcher) : base(receiver, dispatcher) { }
        public NetworkAnalyser(IDispatcher dispatcher) : base(dispatcher) { }

        public override void Analyse(Message message)
        {
            //TODO: need a better design here... this switch will become 
            //unmanageable soon
            switch (message.RoutingKey)
            {

                case TCP.TCP_RECONNECT:
                    ProcessTcpReconnect(message);
                    break;

                case DNS.DNS_NAME_ERROR:
                    ProcessDNSErrors(message,NETWORK_COULD_NOT_RESOLVE_NAME);
                    break;

                case DNS.DNS_TIMED_OUT:
                    ProcessDNSErrors(message,NETWORK_DNS_QUERY_TIMEOUT);
                    break;

                default:
                    Debug.Fail("not prepared to analyse " + message.RoutingKey);
                    //TODO: log
                    break;

            }

            Console.WriteLine(message.ToJson());
        }

        private void ProcessDNSErrors(Message message,string error)
        {
            _dispatcher.Send(message.ProcessId, message.Body, error);
        }

        private void ProcessTcpReconnect(Message message)
        {
            if (_tcpEvents.ContainsKey(message.ToString())) //HACK: use a computed hash instead?
            {
                //ok there is an error. the application could not connect to destination twice.

                //Send error down the pipe
                _dispatcher.Send(message.ProcessId, message.Body, NETWORK_UNREACHABLE_DESTINATION);
                Console.WriteLine(" TCP failed to connect to " + message.ToJson());

                //no need to keep this as we are now on the second occurrence of same error of the same process
                _tcpEvents.Remove(message.ToString());

            }

            //on the first event take no further action
            _tcpEvents.Add(message.ToString(), message);//TODO: the message must expire itself in a timely manner
        }

        public override IEnumerable<string> PrefixesToSubscribeTo()
        {
            return new[] {  TCP.TCP_RECONNECT,
                            DNS.DNS_NAME_ERROR,
                            DNS.DNS_TIMED_OUT
                            };

        }
    }
}
