using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ZeroMQ;

namespace Servicedog
{
    public class MessageDispatcher: IDispatcher
    {
        private ZSocket _sock;

        public MessageDispatcher()
        {
            var middleware = ZeroMiddleware.Instace;
            _sock = middleware.Publisher;
        }

        public void Send(string body, string routingKey/*, Type type*/)
        {
            using (var message = new ZMessage())//HACK: can we reuse instance between events?
            {
                message.Add(new ZFrame(routingKey)); //envelope
                message.Add(new ZFrame(body));//body
                //message.Add(new ZFrame(type.FullName));//deserialization info
                _sock.Send(message);
            }
        }

        public void Start(ZContext ctx,string queueAddress,string routingKey)
        {
            using (var subscriber = new ZSocket( ctx, ZSocketType.SUB))
            {
                subscriber.Connect(queueAddress);
                subscriber.Subscribe(routingKey);
                var stop = false;
                while (!stop)
                {
                    using (ZMessage message = subscriber.ReceiveMessage())
                    {

                        // Read envelope with address
                        string key = message[0].ReadString();

                        // Read message contents
                        ulong id = message[1].ReadUInt64();

                        // Read message contents
                        string contents = message[2].ReadString();


                        Console.WriteLine("{0}. [{1}] {2} {3}", routingKey, key, id, contents);
                    }
                    Console.CancelKeyPress += (object s, ConsoleCancelEventArgs args) => { stop = true; };

                }
            }
        }
        public void Dispatch(byte[] content,Type type)
        {

        }

    }
}
