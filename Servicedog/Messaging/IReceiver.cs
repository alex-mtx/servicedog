using System;
using System.Collections.Generic;

namespace Servicedog.Messaging
{
    public interface IReceiver
    {
        Message NextMessage();
        void AddRoutingKey(string routingKey);
        void AddRoutingKeys(IEnumerable<string> routingKeys);
    }
}