namespace Servicedog
{
    public interface IDispatcher
    {
        void Send(string body, string routingKey);
    }
}