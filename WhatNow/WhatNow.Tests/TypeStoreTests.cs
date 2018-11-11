using Microsoft.VisualStudio.TestTools.UnitTesting;
using WhatNow.Contracts.Data;

namespace WhatNow.Tests
{
    [TestClass]
    public class TypeStoreTests
    {
        readonly ActionToken store;

        public TypeStoreTests()
        {
            store = new ActionToken();
        }

        [TestMethod]
        public void StoresDataPositive()
        {
            var value = 1;
            store.Set(value, ItemLifespan.Permanent);

            Assert.AreEqual(value, store.Get<int>());
            Assert.IsTrue(store.Contains<int>());
            Assert.IsTrue(store.TryGet(out int outValue));
            Assert.AreEqual(value, outValue);
        }

        [TestMethod]
        public void StoresDataNegative()
        {
            Assert.IsFalse(store.Contains<double>());
            Assert.IsFalse(store.TryGet(out double outValue2));
            Assert.AreEqual(default(double), outValue2);
        }

        [TestMethod]
        public void UpdatesData()
        {
            var value = new DummyType() { Property = 1};
            store.Set(value, ItemLifespan.Permanent);
            store.Do<DummyType>(d => d.Property = 2);

            Assert.AreEqual(store.Get<DummyType>().Property, 2);
            Assert.IsTrue(store.TryDo<DummyType>(d => d.Property = 3));
            Assert.AreEqual(store.Get<DummyType>().Property, 3);
        }

        class DummyType
        {
            public double Property { get; set; }
        }
    }
}
