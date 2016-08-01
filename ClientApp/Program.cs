using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ClientApp
{
    class Program
    {
        private static readonly Random _switch = new Random();
        static void Main(string[] args)
        {

            var cmd = Console.ReadLine();
            var parameter = "http://localhost";
            var shouldFail = "false";
            var processId = Process.GetCurrentProcess().Id;
            


            Console.WriteLine(processId + " " + cmd);
            var go = true;
            while (go)
            {
                var randomSwitch = _switch.Next(2);
                shouldFail = randomSwitch == 1 ? "true" : "false";

                try
                {

                    switch(cmd)
                    {
                        case "http":

                            ExecuteHttpRequest(parameter, bool.Parse(shouldFail));
                            Console.WriteLine("http request ok");
                            break;

                        case "tcpf":
                            MultipleFailureTelnet();
                            go=false;
                            break;
                        default:
                            break;
                    }
                }
                catch (Exception)
                {

                    Console.WriteLine("Exception raised");
                }


                Thread.Sleep(1000);
            }
            Console.WriteLine("crtl+c");
            Console.Read();
        }



       
        private static void ExecuteHttpRequest(string url, bool shouldFail)
        {
            var result = string.Empty;
            if (shouldFail)
                url = url.Replace("localhost", "error" + DateTime.Now.Ticks.ToString()); //need to have a new host name on every call because of the dns cache

            Console.WriteLine(url);

            var client = new HttpClient { Timeout = new TimeSpan(0,0,3) };
            result = client.GetStringAsync(url).Result;
        }

        private static void MultipleFailureTelnet()
        {
            //TODO: get this working... shame on me :-(
            Console.WriteLine("telnet");
            Parallel.For(0, 10000, delegate(int it) 
            {
                ProcessStartInfo proc =
                new ProcessStartInfo(@"c:\windows\system32\telnet.exe", "localhost 99");
                proc.CreateNoWindow = true;
                proc.UseShellExecute = true;
                try
                {
                    Process.Start(proc);
                }catch (Exception e)
                {
                    Console.WriteLine("Error detected. are you an admin?");
                    Console.WriteLine(e);
                    throw;
                }
            });
        }

    }
}
