using System;

namespace WhatNow.Contracts.Resources.Monitoring
{
    public interface IAccessMonitor
    {
        void AccessEvent(ResourceDefinition resource);
        void AccessTimeoutEvent(ResourceDefinition resource, TimeSpan timeout);
        void AccessWaitEvent(ResourceDefinition resource, TimeSpan wait);
        void ExitEvent(ResourceDefinition resource, TimeSpan holdDuration);
        void ResourceCreatedEvent(ResourceDefinition resource);
        void ResourceDisposedEvent(ResourceDefinition resource);

        void Clear();
    }
}