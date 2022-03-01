namespace WhatNow.Essentials.Resources.Monitoring
{
    using System;
    using System.Collections.Generic;
    using WhatNow.Contracts.Resources;

    public class AccessMonitor
    {
        readonly object eventsLock = new object();
        readonly List<Event> events = new List<Event>();

        readonly object accessLock = new object();
        readonly HashSet<Enum> runningAccesses = new HashSet<Enum>();

        public void StartAccess(ResourceDefinition resource)
        {
            lock (accessLock)
                runningAccesses.Add(resource.Id);
        }

        public void EndAccess(ResourceDefinition resource)
        {
            lock (accessLock)
                runningAccesses.Remove(resource.Id);
        }

        public bool AccessExists(ResourceDefinition resource)
        {
            lock (accessLock)
                return runningAccesses.Contains(resource.Id);
        }

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
            lock (eventsLock)
                events.Add(@event);
        }
    }
}
