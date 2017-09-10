using System.Threading;

namespace Servicedog.Watchers
{
    public interface IWatcher
    {
        void StartWatching(CancellationToken cancellation);
    }
}