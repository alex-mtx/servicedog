using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Topshelf;
using ZeroMQ;

namespace Servicedog
{
    class Program
    {
        static void Main(string[] args)
        {

            HostFactory.New(x =>
            {
                x.SetServiceName("servicedog");
                x.SetDescription("Captures and analyses problems that interfere on the communications between local and remote applications.");
                x.Service<ServiceRunner>(sc =>
                {
                    sc.ConstructUsing(() => new ServiceRunner());
                    sc.WhenStarted(s => s.Start());
                    sc.WhenStopped(s => s.Stop());
                    sc.WhenShutdown(s => s.Shutdown());
                });

            });

        }

    }
}
