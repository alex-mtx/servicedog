namespace Servicedog.Messaging
{
    public class Message
    {
        public int ProcessId { get; set; }
        public string RoutingKey { get; set; }
        public string Body { get; set; }

        public string ToJson()
        {
            return "{ \"process_id\":" + ProcessId + ", \"routing_key\":\"" + RoutingKey +"\",\"body\":\"" + Body + "\"";
        }

        public override string ToString()
        {
            return ProcessId + " " + RoutingKey + " " + Body;
        }
    }
}
