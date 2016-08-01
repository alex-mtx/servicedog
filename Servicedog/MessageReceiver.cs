using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZeroMQ;

namespace Servicedog
{
    /// <summary>
    /// Middleware client that acts as a Subscriber.
    /// </summary>
    public class MessageReceiver : IMessageReceiver
    {
        private ZSocket _queue;
        private ICollection<string> _keys = new List<string>();
        private ZMessage _currentMessage;
       
        public MessageReceiver(ICollection<string> routingKeys)
        {
            _keys = routingKeys;
            _queue = ZeroMiddleware.Instace.CreateConsumer(routingKeys);
        }

        public MessageReceiver(string routingKey) : this(new string[] { routingKey }){ }

        public Tuple<string, string> NextMessage()
        {

            _currentMessage = _queue.ReceiveMessage();

            return new Tuple<string, string>(_currentMessage[0].ReadString(),
               _currentMessage[1].ReadString());


        }



    }
}
