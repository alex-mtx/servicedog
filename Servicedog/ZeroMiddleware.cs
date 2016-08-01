using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZeroMQ;

namespace Servicedog
{
    public class ZeroMiddleware : IDisposable
    {
        private static volatile ZeroMiddleware instance;
        public  ZContext Queue { get; private set; }
        private static object syncRoot = new object();
        private static volatile bool initialized;
        private static string _queueAddress = "inproc://events";
        public ZSocket Publisher { get; private set; }
        private List<ZSocket> _subscribers = new List<ZSocket>();
        

        private ZeroMiddleware()
        {
            Queue = new ZContext();
            Publisher = new ZSocket(Queue, ZSocketType.PUB);
            Publisher.Bind(_queueAddress);
            initialized = true;
        }

        public static ZeroMiddleware Instace
        {
            get
            {
                lock (syncRoot)
                {
                    if (initialized) return instance;
                    instance = new ZeroMiddleware();
                    return instance;
                }
            }
        }
        public ZSocket CreateConsumer(string routingKey)
        {
            var keys = new string[] { routingKey };
            return CreateConsumer(keys);
        }
        public ZSocket CreateConsumer(ICollection<string> routingKeys)
        {

            if (routingKeys == null)
                routingKeys = new List<string>();

            var subscriber = new ZSocket(Queue, ZSocketType.SUB);
            subscriber.Connect(_queueAddress);

            foreach (var key in routingKeys)
            {
                subscriber.Subscribe(key);
            }

            _subscribers.Add(subscriber);

            return subscriber;
        }



        public void Dispose()
        {
            _subscribers.ForEach(x => x.Dispose());
            Publisher.Dispose();
            Queue.Dispose();
        }

    }
}
