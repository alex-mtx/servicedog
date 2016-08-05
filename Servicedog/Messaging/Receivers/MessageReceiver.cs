using System;
using System.Collections.Generic;
using ZeroMQ;

namespace Servicedog.Messaging.Receivers
{
    /// <summary>
    /// Middleware client that acts as a Subscriber.
    /// </summary>
    public class MessageReceiver : IReceiver
    {
        private ZSocket _queue;
        private ZMessage _currentMessage;
        private const int ROUTING_KEY_POSITION = 0;
        private const int PROCESS_ID_POSITION = 1;
        private const int BODY_POSITION = 2;

        public MessageReceiver()
        {
            _queue = ZeroMiddleware.Instace.CreateConsumer();
        }
        public MessageReceiver(ICollection<string> routingKeys)
        {
            _queue = ZeroMiddleware.Instace.CreateConsumer(routingKeys);
            AddRoutingKeys(routingKeys);
        }

        public MessageReceiver(string routingKey)
        {
            _queue = ZeroMiddleware.Instace.CreateConsumer(routingKey);
            AddRoutingKey(routingKey);

        }

        public Message NextMessage()
        {

            _currentMessage = _queue.ReceiveMessage();//HACK: need to reuse ZMessage to save resources

            return new Message
            {
                ProcessId = _currentMessage[PROCESS_ID_POSITION].ReadInt32(),
                Body = _currentMessage[BODY_POSITION].ReadString(),
                RoutingKey = _currentMessage[ROUTING_KEY_POSITION].ReadString()
            };

        }

        public void AddRoutingKey(string routingKey)
        {
            _queue.Subscribe(routingKey);
        }

        public void AddRoutingKeys(IEnumerable<string> routingKeys)
        {
            foreach (var key in routingKeys)
            {
                _queue.Subscribe(key);
            }
        }
    }
}
