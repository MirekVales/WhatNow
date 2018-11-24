using System;

namespace WhatNow.Contracts.Resources
{
    public abstract class IResourceAccess<T> : IDisposable
        where T : IAccessableResource
    {
        public abstract DateTime Created { get; }
        public abstract ResourceDefinition Definition { get; }
        public abstract T Resource { get; }

        public abstract void Dispose();
    }
}
