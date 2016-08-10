using Servicedog.Utils;

namespace Servicedog.Messaging
{
    public class Message
    {
        public ProcessInfo Process { get; set; }

        public int ProcessId { get; set; }
        public string RoutingKey { get; set; }
        public string Body { get; set; }

        public string ToJson()
        {
            return "{ \"process\":"+ Process.ToString() + ", \"routing_key\":\"" + RoutingKey +"\",\"body\":\"" + Body + "\"";
        }

        public override string ToString()
        {
            return ProcessId + " " + RoutingKey + " " + Body;
        }
    }
}
