using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZeroMQ;

namespace Servicedog.Messaging
{
    public class ZeroMiddleware : IDisposable
    {
        private static volatile ZeroMiddleware instance;
        public ZContext Queue { get; private set; }
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
            var subscriber = NewConsumer();
            if (!string.IsNullOrEmpty(routingKey))
                subscriber.Subscribe(routingKey);

            return subscriber;
        }

        /// <summary>
        /// Use it when you need to define your subscription in another step.
        /// </summary>
        /// <returns></returns>
        public ZSocket CreateConsumer()
        {
            return NewConsumer();
        }

        /// <summary>
        /// Subscribes to all keys present is the <paramref name="routingKeys"/>
        /// When <paramref name="routingKeys"/> is null, you'll be 
        /// </summary>
        /// <param name="routingKeys"></param>
        /// <returns></returns>
        public ZSocket CreateConsumer(ICollection<string> routingKeys)
        {
            if (routingKeys == null)
                routingKeys = new List<string>();
            var subscriber = NewConsumer();
            foreach (var key in routingKeys)
                subscriber.Subscribe(key);
            return subscriber;
        }
        //call it before calling ZSocket.Subscribe, 
        //otherwise your subscription won't take any effect.
        private ZSocket NewConsumer()
        {
            var subscriber = new ZSocket(Queue, ZSocketType.SUB);
            subscriber.Connect(_queueAddress);
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
