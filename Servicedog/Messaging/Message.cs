using Servicedog.OS;

namespace Servicedog.Messaging
{
    public class Message : IMessage
    {
        public IProcessInfo Process { get; set; }

        public int ProcessId { get; set; }
        public string RoutingKey { get; set; }
        public string Body { get; set; }

        public string ToJson()
        {
            return "{ \"process\":"+ Process.ToJson() + ", \"routing_key\":\"" + RoutingKey +"\",\"body\":\"" + Body + "\"}";
        }

        public override string ToString()
        {
            return ProcessId + " " + RoutingKey + " " + Body;
        }
    }
}
