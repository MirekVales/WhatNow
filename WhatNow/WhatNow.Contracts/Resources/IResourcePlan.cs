using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WhatNow.Contracts.Resources
{
    public interface IResourcePlan
    {
        ResourceDefinition this[Enum id] { get; }

        Dictionary<Enum, ResourceDefinition> GetResourceInfo();

        IResourcePlan With(Enum id, Func<IResourcePlan, ResourceDefinition> resourceFunc);
        IResourcePlan WithThreadPool(Enum @enum, int numberOfThreads, TaskCreationOptions taskCreationOptions = TaskCreationOptions.None);
    }
}