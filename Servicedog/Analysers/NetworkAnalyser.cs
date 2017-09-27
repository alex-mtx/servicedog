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
        public const string NETWORK_NEW_CONNECTION = "NETWORK_NEW_CONNECTION";
        public const string NETWORK_END_CONNECTION = "NETWORK_END_CONNECTION";
        public const string SOCKET_ABORT= "SOCKET_ABORT";
        public const string SOCKET_CONNECT= "SOCKET_CONNECT";
        public const string SOCKET_CAN_NOT_CONNECT = "SOCKET_CAN_NOT_CONNECT";

        private readonly IDictionary<string, IMessage> _tcpEvents = new Dictionary<string, IMessage>();

        public NetworkAnalyser(IReceiver receiver, IDispatcher dispatcher) : base(receiver, dispatcher) { }
        public NetworkAnalyser(IDispatcher dispatcher) : base(dispatcher) { }

        public override void Analyse(IMessage message)
        {
            //TODO: need a better design here... this switch will become 
            //unmanageable soon
            switch (message.RoutingKey)
            {

                case TcpWatcher.TCP_RECONNECT:
                    ProcessTcpReconnect(message);
                    break;

                case TcpWatcher.TCP_CONNECT:
                    FanOutEvent(message, NETWORK_NEW_CONNECTION);
                    break;

                case TcpWatcher.TCP_DISCONNECT:
                    FanOutEvent(message, NETWORK_END_CONNECTION);
                    break;

                case DnsWatcher.DNS_NAME_ERROR:
                    FanOutEvent(message,NETWORK_COULD_NOT_RESOLVE_NAME);
                    break;

                case DnsWatcher.DNS_TIMED_OUT:
                    FanOutEvent(message,NETWORK_DNS_QUERY_TIMEOUT);
                    break;

                case WinsockWatcher.ABORT:
                    FanOutEvent(message, SOCKET_ABORT);
                    break;
                case WinsockWatcher.CONNECT:
                    FanOutEvent(message, SOCKET_CONNECT);
                    break;
                case WinsockWatcher.ERROR_ON_CONNECT:
                    FanOutEvent(message, SOCKET_CAN_NOT_CONNECT);
                    break;

                default:
                    Debug.Fail("not prepared to analyse " + message.RoutingKey);
                    //TODO: log
                    break;

            }

            Debug.WriteLine(message.ToJson());
        }

        private void FanOutEvent(IMessage message,string routingKey)
        {
            _dispatcher.Send(message.ProcessId, message.Body, routingKey);
        }

        private void ProcessTcpReconnect(IMessage message)
        {
            if (_tcpEvents.ContainsKey(message.ToString())) //HACK: use a computed hash instead?
            {
                //ok there is an error. the application could not connect to destination twice.

                //Send error down the pipe
                _dispatcher.Send(message.ProcessId, message.Body, NETWORK_UNREACHABLE_DESTINATION);
                Debug.WriteLine(" TCP failed to connect to " + message.ToJson());

                //no need to keep this as we are now on the second occurrence of same error of the same process
                _tcpEvents.Remove(message.ToString());
                return;
            }

            //on the first event take no further action
            _tcpEvents.Add(message.ToString(), message);//TODO: the message must expire itself in a timely manner
        }

        public override IEnumerable<string> PrefixesToSubscribeTo()
        {
            return new[] {  TcpWatcher.TCP_RECONNECT,
                            DnsWatcher.DNS_NAME_ERROR,
                            DnsWatcher.DNS_TIMED_OUT,
                            WinsockWatcher.ABORT,
                            WinsockWatcher.CONNECT,
                            WinsockWatcher.ERROR_ON_CONNECT,
                            TcpWatcher.TCP_CONNECT,
                            TcpWatcher.TCP_DISCONNECT
                            };

        }
    }
}
