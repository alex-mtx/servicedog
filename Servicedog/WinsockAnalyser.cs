using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Servicedog
{
    public class WinsockAnalyser
    {
        private IMessageReceiver _receiver;

        public WinsockAnalyser(IMessageReceiver receiver)
        {
            _receiver = receiver;

        }

        public void Analyse(CancellationToken cancel)
        {
            while (cancel.IsCancellationRequested)
            {
                Tuple<string, string> message = _receiver.NextMessage();
                Console.WriteLine(message.ToString());
            }

        }
    }
}
