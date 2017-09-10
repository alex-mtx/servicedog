using System.Collections.Generic;
using System.Threading;
using Servicedog.Messaging;

namespace Servicedog.Analysers
{
    public interface IAnalyser
    {
        void Analyse(IMessage message);
        IEnumerable<string> PrefixesToSubscribeTo();
        void StartAnalysing(CancellationToken cancel);
    }
}