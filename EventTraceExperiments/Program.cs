using Microsoft.Diagnostics.Tracing;
using Microsoft.Diagnostics.Tracing.Parsers;
using Microsoft.Diagnostics.Tracing.Parsers.Kernel;
using Microsoft.Diagnostics.Tracing.Parsers.MicrosoftWindowsDNSClient;
using Microsoft.Diagnostics.Tracing.Session;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventTraceExperiments
{
    class Program
    {
        static void Main()
        {

            //Task.Run(()=> DNS.FailedQuery());
            //Task.Run(() => DNS.DNSTimeout());
            //Task.Run(() => DNS.All());
            //Task.Run(() => CLR.ExceptionRaised());
            Task.Run(() => CLR.ExceptionCatchStart());

            Console.Read();
        }
    }
}
