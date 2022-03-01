namespace WhatNow.Essentials.Resources
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Collections.Generic;
    using WhatNow.Contracts.Resources;
    using WhatNow.Essentials.ResourceTypes;

    public class ResourcePlan : IResourcePlan
    {
        public static IResourcePlan Empty => new ResourcePlan();

        readonly Dictionary<Enum, Func<IResourcePlan, ResourceDefinition>> resources
            = new Dictionary<Enum, Func<IResourcePlan, ResourceDefinition>>();

        public ResourceDefinition this[Enum id]
        {
            get => resources[id](this);
        }

        public IResourcePlan WithThreadPool(
            Enum @enum
            , int numberOfThreads
            , TaskCreationOptions taskCreationOptions = TaskCreationOptions.None)
            => With(@enum, _ => new ThreadPoolResourceDefinition(@enum, numberOfThreads, taskCreationOptions));

        public IResourcePlan With(Enum id, Func<IResourcePlan, ResourceDefinition> resourceFunc)
        {
            if (resources.ContainsKey(id))
                throw new Exception($"Resource is already binded to {id}");

            resources[id] = resourceFunc;
            return this;
        }

        public Dictionary<Enum, ResourceDefinition> GetResourceInfo()
            => resources.ToDictionary(k => k.Key, v => v.Value(this));
    }
}
