using System;
using WhatNow.Contracts.Resources;

namespace WhatNow.Essentials.Resources.Monitoring
{
    public class AccessStopwatch
    {
        public DateTime Created { get; }
        public AccessMonitor Monitor { get; }
        public ResourceDefinition Resource { get; }
        public TimeSpan Threshold { get; }

        public AccessStopwatch(AccessMonitor monitor, ResourceDefinition resource)
        {
            Created = DateTime.Now;
            Monitor = monitor;
            Resource = resource;
            Threshold = TimeSpan.FromMilliseconds(15);
        }

        public AccessStopwatch(AccessMonitor monitor, ResourceDefinition resource, TimeSpan threshold)
        {
            Created = DateTime.Now;
            Monitor = monitor;
            Resource = resource;
            Threshold = threshold;
        }

        public void Evaluate()
        {
            var diff = (DateTime.Now - Created);
            if (diff > Threshold)
                Monitor.AccessWaitEvent(Resource, diff);
        }
    }
}
