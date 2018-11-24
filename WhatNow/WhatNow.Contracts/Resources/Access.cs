using System;

namespace WhatNow.Contracts.Resources
{
    public class Access<T> : IResourceAccess<T>
        where T : IAccessableResource
    {
        public override DateTime Created { get; }
        public override ResourceDefinition Definition { get; }
        public override T Resource { get; }

        readonly IResourceManager resourceManager;

        public Access(IResourceManager resourceManager, ResourceDefinition resource, T accessableResource)
        {
            Created = DateTime.Now;
            Definition = resource;
            Resource = accessableResource;
            this.resourceManager = resourceManager;
        }

        public override void Dispose()
            => resourceManager.Exit(Definition, Resource, Created);
    }
}
