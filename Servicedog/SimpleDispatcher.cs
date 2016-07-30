using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ZeroMQ;

namespace Servicedog
{
    static class SimpleDispatcher
    {
        public static void Start(ZContext ctx,string queueAddress,string routingKey)
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
    }
}
