using System;
using ZeroMQ;

namespace Servicedog.Messaging.Dispatchers
{
    public class MessageDispatcher: IDispatcher
    {
        private static ZSocket _sock;

        public MessageDispatcher()
        {
            var middleware = MessageMiddleware.Instace;
            _sock = middleware.Publisher;
        }

        public void Send(Message message, string routingKey)
        {
            using (var zmessage = new ZMessage())//HACK: can we reuse instance between events?
            {
                zmessage.Add(new ZFrame(routingKey)); //envelope name
                zmessage.Add(new ZFrame(message.ProcessId)); //process id
                zmessage.Add(new ZFrame(message.ToJson()));//body
                zmessage.Add(new ZFrame(message.ToJson()));//controls whether this is a string orl
                _sock.Send(zmessage);
            }
        }

        public void Send(int processId,string body, string routingKey)
        {
            using (var message = new ZMessage())//HACK: can we reuse instance between events?
            {
                message.Add(new ZFrame(routingKey)); //envelope name
                message.Add(new ZFrame(processId)); //process id
                message.Add(new ZFrame(body));//body
                _sock.Send(message);
            }
        }
    }
}
