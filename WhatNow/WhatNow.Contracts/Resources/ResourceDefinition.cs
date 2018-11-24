using System;

namespace WhatNow.Contracts.Resources
{
    public abstract class ResourceDefinition
    {
        public Enum Id { get; }
        public string Name => Enum.GetName(Id.GetType(), Id); 

        protected ResourceDefinition(Enum id)
            => Id = id;

        public abstract IAccessableResource Construct();
    }
}
