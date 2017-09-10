using Servicedog.OS;

namespace Servicedog.Messaging
{
    public interface IMessage
    {
        string Body { get; set; }
        IProcessInfo Process { get; set; }
        int ProcessId { get; set; }
        string RoutingKey { get; set; }

        string ToJson();
        string ToString();
    }
}