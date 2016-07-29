using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Servicedog
{
    class Program
    {
        static void Main(string[] args)
        {
            //Task.Run(() => DNS.FailedQuery());
            //Task.Run(() => DNS.DNSTimeout());
            //Task.Run(() => CLR.ExceptionRaised());
            //Task.Run(() => CLR.ExceptionCatchStart());
            //Task.Run(() => TCP.Reconnect());
            Task.Run(() => Winsock.Failed());


            Console.Read();
        }

        private static void SetUpMessaging()
        {
            using (var context = new ZContext())
            using (var subscriber = new ZSocket(context, ZSocketType.XSUB))
            using (var publisher = new ZSocket(context, ZSocketType.XPUB))
            using (var listener = new ZSocket(context, ZSocketType.PAIR))
            {
                new Thread(() => Espresso_Publisher(context)).Start();
                new Thread(() => Espresso_Subscriber(context)).Start();
                new Thread(() => Espresso_Listener(context)).Start();

                subscriber.Connect("tcp://127.0.0.1:6000");
                publisher.Bind("tcp://*:6001");
                listener.Bind("inproc://listener");

                ZError error;
                if (!ZContext.Proxy(subscriber, publisher, listener, out error))
                {
                    if (error == ZError.ETERM)
                        return; // Interrupted
                    throw new ZException(error);
                }
            }
        }
       
    }
}
