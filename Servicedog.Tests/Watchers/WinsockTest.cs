using Moq;
using NUnit.Framework;
using Servicedog.Messaging;
using Servicedog.Watchers;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Servicedog.Tests.Watchers
{
    [TestFixture]
    public class WinsockTest
    {
        /// <summary>
        /// this is a tricky test. the DNS must resolve the
        /// </summary>
        [Test(TestOf = typeof(WinsockWatcher))]
        public void When_A_New_Socket_Can_Not_Connect_Should_Notify_An_Abort_Via_Dispatcher()
        {
            // use always IP addresses or a valid and resolvable DNS entry, otherwise the problem will be a DNS error instead a TCP one. 
            //Keep in mind that if the hostname could not be resolved, the first error will be a DNS, not a TCP.
            string unavailableService = "127.0.0.1:59999";

            string unavailableServiceUrl = "http://" + unavailableService; 
            var cancel = new CancellationToken();
            var events = new List<Message>();
            var dispatcherMoq = WatcherTest.PrepareMock(events);
                         
            var winsock = new WinsockWatcher(dispatcherMoq.Object);

            //act
            winsock.StartWatching(cancel);
            Thread.Sleep(2000);
            //now we force a tcp connection to nowhere
            CallService(unavailableServiceUrl);

            //give some room to ETW to raise the Tcp Event
           Thread.Sleep(4000);

            WatcherTest.AssertSendCalled(dispatcherMoq, Times.AtLeastOnce());
            WatcherTest.AssertExpectedEventSent(events, WinsockWatcher.ABORT, string.Empty);
        }

        [Test(TestOf = typeof(WinsockWatcher))]
        public void When_A_New_Socket_Connects_Should_Notify_An_Connect_Via_Dispatcher()
        {

            var cancel = new CancellationToken();

            var events = new List<Message>();
            var dispatcherMoq = WatcherTest.PrepareMock(events);

            var winsock = new WinsockWatcher(dispatcherMoq.Object);
            //act
            winsock.StartWatching(cancel);

            //give some room to ETW to raise the Tcp Event
            Thread.Sleep(4000);
            CreateSocketListener(cancel);
            Connect();

            WatcherTest.AssertSendCalled(dispatcherMoq, Times.AtLeastOnce());
            WatcherTest.AssertExpectedEventSent(events, WinsockWatcher.CONNECT, string.Empty);
        }

        static void Connect()
        {
            try
            {
                // Create a TcpClient.
                // Note, for this client to work you need to have a TcpServer 
                // connected to the same address as specified by the server, port
                // combination.
                Int32 port = 13000;
                TcpClient client = new TcpClient("127.0.0.1", port);

                // Translate the passed message into ASCII and store it as a Byte array.
                Byte[] data = System.Text.Encoding.ASCII.GetBytes("test");

                // Get a client stream for reading and writing.
                //  Stream stream = client.GetStream();

                NetworkStream stream = client.GetStream();

                // Send the message to the connected TcpServer. 
                stream.Write(data, 0, data.Length);

                stream.Close();
                client.Close();
            }
            catch (ArgumentNullException e)
            {
                Console.WriteLine("ArgumentNullException: {0}", e);
            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException: {0}", e);
            }

            Console.WriteLine("\n Press Enter to continue...");
            Console.Read();
        }

        private void CreateSocketListener(CancellationToken cancel)
        {
            Task.Run(() =>
            {
                TcpListener server = null;
                try
                {
                    // Set the TcpListener on port 13000.
                    Int32 port = 13000;
                    IPAddress localAddr = IPAddress.Parse("127.0.0.1");

                    // TcpListener server = new TcpListener(port);
                    server = new TcpListener(localAddr, port);

                    // Start listening for client requests.
                    server.Start();

                    // Buffer for reading data
                    Byte[] bytes = new Byte[256];
                    String data = null;

                    // Enter the listening loop.
                    while (true)
                    {
                        Console.Write("Waiting for a connection... ");

                        // Perform a blocking call to accept requests.
                        // You could also user server.AcceptSocket() here.
                        TcpClient client = server.AcceptTcpClient();
                        Console.WriteLine("Connected!");

                        data = null;

                        // Get a stream object for reading and writing
                        NetworkStream stream = client.GetStream();

                        int i;
                        Thread.Sleep(1000);
                        // Loop to receive all the data sent by the client.
                        //while ((i = stream.Read(bytes, 0, bytes.Length)) != 0)
                        //{
                        //    // Translate data bytes to a ASCII string.
                        //    data = System.Text.Encoding.ASCII.GetString(bytes, 0, i);
                        //    Console.WriteLine("Received: {0}", data);

                        //    // Process the data sent by the client.
                        //    data = data.ToUpper();

                        //    byte[] msg = System.Text.Encoding.ASCII.GetBytes(data);

                        //    // Send back a response.
                        //    stream.Write(msg, 0, msg.Length);
                        //    Console.WriteLine("Sent: {0}", data);
                        //}

                        // Shutdown and end connection
                        client.Close();
                    }
                }
                catch (SocketException e)
                {
                    Console.WriteLine("SocketException: {0}", e);
                }
                finally
                {
                    // Stop listening for new clients.
                    server.Stop();
                }


            }, cancel);
        }
        private static void CallService(string unavailableServiceUrl)
        {
            try
            {

                using (var cli = new HttpClient())
                {
                    cli.Timeout = new TimeSpan(0, 0, 1);
                    var asyncTask = cli.GetAsync(new Uri(unavailableServiceUrl));
                    asyncTask.Wait();
                }
            }
            catch 
            {
               //swallow it
            }
        }
    }
}
