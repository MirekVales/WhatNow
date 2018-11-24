using System;
using WhatNow.Contracts.Resources;

namespace WhatNow.Essentials.Resources.Monitoring
{
    public struct Event
    {
        public DateTime Created { get; }
        public EventType EventType { get; }
        public ResourceDefinition AccessedResource { get; }
        public TimeSpan Duration { get; }

        public Event(EventType eventType, ResourceDefinition accessedResource, TimeSpan duration)
        {
            Created = DateTime.Now;
            EventType = eventType;
            AccessedResource = accessedResource;
            Duration = duration;
        }
    }
}
