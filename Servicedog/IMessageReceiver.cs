using System;

namespace Servicedog
{
    public interface IMessageReceiver
    {
        Tuple<string, string> NextMessage();
    }
}