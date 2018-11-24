using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using WhatNow.Contracts.Resources;
using WhatNow.Contracts.Resources.Accessable;
using WhatNow.Essentials.Resources;

namespace WhatNow.Tests
{
    [TestClass]
    public class ResourceManagementTests
    {
        internal enum Resource
        {
            Threads1
            , Threads2
        }

        [TestMethod]
        public void WaitsForResources()
        {
            var plan = new ResourcePlan()
                .WithThreadPool(Resource.Threads1, 1)
                .WithThreadPool(Resource.Threads2, 2);

            ResourceManager manager;
            IResourceAccess<IThreadPool> access;

            using (manager = new ResourceManager(plan))
            {
                Assert.ThrowsException<InvalidOperationException>(() => manager.Access<IThreadPool>(Resource.Threads1));

                manager.AllocateResources();

                using (access = manager.Access<IThreadPool>(Resource.Threads1))
                {
                    Assert.AreEqual(1, access.Resource.Threads);
                }

                using (access = manager.Access<IThreadPool>(Resource.Threads2))
                {
                    Assert.AreEqual(2, access.Resource.Threads);
                }

                using (access = manager.Access<IThreadPool>(Resource.Threads1))
                {
                    Assert.AreEqual(1, access.Resource.Threads);
                }
            }

            Assert.ThrowsException<InvalidOperationException>(() => manager.Access<IThreadPool>(Resource.Threads1));
        }
    }
}
