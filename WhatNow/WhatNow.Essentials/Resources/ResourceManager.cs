using System;
using System.Collections.Generic;
using WhatNow.Contracts.Resources;
using WhatNow.Essentials.Resources.Monitoring;

namespace WhatNow.Essentials.Resources
{
    public class ResourceManager : IResourceManager
    {
        public AccessMonitor AccessMonitor;

        public IResourcePlan ResourcePlan { get; private set; }

        Dictionary<Enum, IAccessableResource> resources;
        bool initialized;

        public ResourceManager()
        {
            AccessMonitor = new AccessMonitor();
            resources = new Dictionary<Enum, IAccessableResource>();
        }

        public ResourceManager(IResourcePlan plan)
        {
            AccessMonitor = new AccessMonitor();
            resources = new Dictionary<Enum, IAccessableResource>();
            ResourcePlan = plan;
        }

        public IDisposable CreateUseScope(Action<IResourceManager> initiationAssignment)
        {
            initiationAssignment(this);

            return null;
        }

        public void AllocateResources()
        {
            if (ResourcePlan == null || initialized)
                return;

            AccessMonitor = new AccessMonitor();

            foreach (var pair in ResourcePlan.GetResourceInfo())
            {
                resources.Add(pair.Key, pair.Value.Construct());
                AccessMonitor.ResourceCreatedEvent(pair.Value);
            }

            initialized = true;
        }

        public void SetPlan(ResourcePlan plan)
        {
            FreeResources();

            ResourcePlan = plan;

            AllocateResources();
        }

        public void FreeResources()
        {
            if (ResourcePlan == null || !initialized)
                return;

            foreach (var resource in resources)
            {
                resource.Value.Dispose();
                AccessMonitor.ResourceDisposedEvent(ResourcePlan[resource.Key]);
            }
            resources.Clear();

            ResourcePlan = null;
            initialized = false;
        }

        public IResourceAccess<T> Access<T>(Enum resourceToAllocate)
            where T : IAccessableResource
        {
            if (!initialized)
                throw new InvalidOperationException("Resources were not initialized yet");

            var resource = ResourcePlan[resourceToAllocate];
            var accessWait = new AccessStopwatch(AccessMonitor, resource);
            accessWait.Evaluate();
            AccessMonitor.AccessEvent(resource);
            return new Access<T>(this, resource, (T)resources[resourceToAllocate]);
        }

        public void Exit(ResourceDefinition definition, IAccessableResource resource, DateTime creationTime)
        {
            AccessMonitor.ExitEvent(definition, DateTime.Now - creationTime);
        }

        public void Dispose()
        {
            FreeResources();
        }
    }
}
