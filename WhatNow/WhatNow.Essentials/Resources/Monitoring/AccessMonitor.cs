using System;
using System.Collections.Generic;
using WhatNow.Contracts.Resources;

namespace WhatNow.Essentials.Resources.Monitoring
{
    public class AccessMonitor
    {
        readonly object accessLock = new object();
        readonly List<Event> events = new List<Event>();

        public void AccessEvent(ResourceDefinition resource)
            => AddEvent(new Event(EventType.Access, resource, TimeSpan.Zero));

        public void AccessTimeoutEvent(ResourceDefinition resource, TimeSpan timeout)
            => AddEvent(new Event(EventType.AccessTimeout, resource, timeout));

        public void AccessWaitEvent(ResourceDefinition resource, TimeSpan wait)
            => AddEvent(new Event(EventType.AccessWait, resource, wait));

        public void ExitEvent(ResourceDefinition resource, TimeSpan holdDuration)
            => AddEvent(new Event(EventType.Exit, resource, holdDuration));

        public void ResourceCreatedEvent(ResourceDefinition resource)
            => AddEvent(new Event(EventType.ResourceCreated, resource, TimeSpan.Zero));

        public void ResourceDisposedEvent(ResourceDefinition resource)
            => AddEvent(new Event(EventType.ResourceDisposed, resource, TimeSpan.Zero));

        void AddEvent(Event @event)
        {
            lock (accessLock)
                events.Add(@event);
        }
    }
}
