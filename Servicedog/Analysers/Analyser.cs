using Servicedog.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Servicedog.OS;
using Servicedog.Messaging.Receivers;

namespace Servicedog.Analysers
{
    public abstract class Analyser : IAnalyser
    {
        protected IReceiver _receiver;
        protected IDispatcher _dispatcher;
        protected IMessage _message;

        //For testing and injection purposes
        public Analyser(IReceiver receiver, IDispatcher dispatcher)
        {
            _receiver = receiver;
            _dispatcher = dispatcher;
            
        }
        
        public Analyser(IDispatcher dispatcher)
        {
            _dispatcher = dispatcher;
            _receiver = new MessageReceiver();

        }
        /// <summary>
        /// Start listening for new messages and analyse them. All in another Thread.
        /// </summary>
        /// <param name="cancel"></param>
        public void StartAnalysing(CancellationToken cancel)
        {
            try
            {
                _receiver.AddRoutingKeys(PrefixesToSubscribeTo());

                Task.Run(() =>
                {
                    while (true)
                    {
                        _message = _receiver.NextMessage();//this is a blocking call
                        Analyse(_message);
                    }
                }, cancel);
            }
            catch (TaskCanceledException)
            {
                //do nothing, just exit.
            }
            catch (Exception)
            {
                //TODO: need to analyse the behavior of _receiver,NextMessage(). It is still unclear for me how zeromq works.
                throw;
            }
        }

        public abstract void Analyse(IMessage message);
        public abstract IEnumerable<string> PrefixesToSubscribeTo();
    }
}
