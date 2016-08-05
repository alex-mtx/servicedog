namespace Servicedog.Messaging
{
    public interface IDispatcher
    {
        void Send(int processId, string body, string routingKey);
    }
}