using Servicedog.Messaging;
using Servicedog.Watchers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Servicedog.Infrastructure
{
    public static class SetupApp
    {
        public static List<Watcher> InstantiateWatchers(IDispatcher messaging)
        {
            var watcherTypes = Assembly.GetExecutingAssembly().GetTypes().Where(t => t.BaseType == typeof(Watcher)).ToList();
            var watcherIstances = new List<Watcher>();

            watcherTypes.ForEach((Type type) =>
            {
                var ctor = type.GetConstructor(new[] { typeof(IDispatcher) });
                Watcher watcher = ctor.Invoke(new[] { messaging }) as Watcher;
                watcherIstances.Add(watcher);
            });
            return watcherIstances;
        }




    }
}
