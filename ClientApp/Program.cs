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

            Console.WriteLine(Usage());
            Console.ReadLine();
            var proto = "http";
            var parameter = "http://google.com";
            var shouldFail = "false";
            var processId = Process.GetCurrentProcess().Id;
            


            Console.WriteLine(processId);

            while (true)
            {
                var randomSwitch = _switch.Next(2);
                shouldFail = randomSwitch == 1 ? "true" : "false";

                try
                {

                    switch (proto)
                    {
                        case "http":

                            ExecuteHttpRequest(parameter, bool.Parse(shouldFail));
                            Console.WriteLine("http request ok");
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


        }



        private static string Usage()
        {
            return @"[proto (http=1)] [parameter (http://localhost=1)] [shouldFail (false|true)]";
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


    }
}
