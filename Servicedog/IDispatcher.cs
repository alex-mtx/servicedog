namespace Servicedog
{
    public interface IDispatcher
    {
        void Send(object body, string routingKey);
    }
}