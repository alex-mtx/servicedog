using ZeroMQ;

namespace Servicedog.Messaging.Dispatchers
{
    public class MessageDispatcher: IDispatcher
    {
        private ZSocket _sock;

        public MessageDispatcher()
        {
            var middleware = ZeroMiddleware.Instace;
            _sock = middleware.Publisher;
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
