using Helios.Concurrency;
using System;
using System.Threading.Tasks;
using WhatNow.Contracts.Resources;

namespace WhatNow.Essentials.ResourceTypes
{
    public class ThreadPoolResourceDefinition : ResourceDefinition
    {
        readonly int numberOfThreads;
        readonly TaskCreationOptions taskCreationOptions;

        public ThreadPoolResourceDefinition(Enum id, int numberOfThreads, TaskCreationOptions taskCreationOptions)
            : base(id)
        {
            this.numberOfThreads = numberOfThreads;
            this.taskCreationOptions = taskCreationOptions;
        }

        public override IAccessableResource Construct()
        {
            var settings = new DedicatedThreadPoolSettings(numberOfThreads, $"{nameof(WhatNow)}.{nameof(ThreadPoolResourceDefinition)}.{Name}");
            var pool = new DedicatedThreadPool(settings);
            return new ThreadPool(pool, taskCreationOptions);
        }
    }
}
