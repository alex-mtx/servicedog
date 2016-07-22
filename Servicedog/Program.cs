using System;
using System.Collections.Generic;
using System.Linq;
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
            Task.Run(() => CLR.ExceptionRaised());
            //Task.Run(() => CLR.ExceptionCatchStart());
            Task.Run(() => TCP.Reconnect());

            Console.Read();
        }
    }
}
