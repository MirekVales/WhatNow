namespace WhatNow.Essentials.Resources.Monitoring
{
    using System;
    using WhatNow.Contracts.Resources;
    using WhatNow.Contracts.Resources.Monitoring;

    public class AccessMonitorSink : IAccessMonitor
    {
        public void AccessEvent(ResourceDefinition resource)
        {
        }

        public void AccessTimeoutEvent(ResourceDefinition resource, TimeSpan timeout)
        {
        }

        public void AccessWaitEvent(ResourceDefinition resource, TimeSpan wait)
        {
        }

        public void ExitEvent(ResourceDefinition resource, TimeSpan holdDuration)
        {
        }

        public void ResourceCreatedEvent(ResourceDefinition resource)
        {
        }

        public void ResourceDisposedEvent(ResourceDefinition resource)
        {
        }

        public void Clear()
        {
        }
    }
}
