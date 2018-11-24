using System;

namespace WhatNow.Contracts.Resources
{
    public interface IResourceManager : IDisposable
    {
        IDisposable CreateUseScope(Action<IResourceManager> initiationAssignment);

        IResourceAccess<T> Access<T>(Enum resourceToAllocate)
             where T : IAccessableResource;

        void Exit(ResourceDefinition definition, IAccessableResource resource, DateTime accessCreation);

        void AllocateResources();
        void FreeResources();
    }
}