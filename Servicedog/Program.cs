using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using ZeroMQ;

namespace Servicedog
{
    class Program
    {
        static void Main(string[] args)
        {
            var queue = "inproc://events";
            using (var context = new ZContext()) { 

                //Task.Run(() => DNS.FailedQuery());
                //Task.Run(() => DNS.DNSTimeout());
                //Task.Run(() => CLR.ExceptionRaised());
                //Task.Run(() => CLR.ExceptionCatchStart());
                //Task.Run(() => TCP.Reconnect());
                Task.Run(() => Winsock.Capture(context)).Wait(1000); //wait for the zmq socket creation 
                Task.Run(() => SimpleDispatcher.Start(context, queue, Winsock.Connect));
                Task.Run(() => SimpleDispatcher.Start(context, queue, Winsock.ErrorOnConnect));
                Console.Read();

            }
        }

    }
}
