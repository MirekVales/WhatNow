using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using WhatNow.Contracts;
using WhatNow.Contracts.Actions;
using WhatNow.Contracts.Exceptions;
using WhatNow.Essentials;

namespace WhatNow.Tests
{
    [TestClass]
    public class ActionPipeMapTests
    {
        readonly ActionPipeMap map;

        public ActionPipeMapTests()
        {
            map = new ActionPipeMap();
        }

        [TestMethod]
        public void AddsTypesPositive()
        {
            map
                .StartsAt<DummyAction1>()
                .Then<DummyAction2>()
                .Then<DummyAction3>();

            Assert.IsTrue(map.UsedActionTypes.Contains(typeof(DummyAction1)));
            Assert.IsTrue(map.UsedActionTypes.Contains(typeof(DummyAction2)));
            Assert.IsTrue(map.UsedActionTypes.Contains(typeof(DummyAction3)));
        }

        [TestMethod]
        public void AddsTypesNegative()
        {
            map
                .StartsAt<DummyAction1>()
                .Then<DummyAction2>()
                .Then<DummyAction3>();

            Assert.IsFalse(map.UsedActionTypes.Contains(typeof(DummyAction4)));
            Assert.IsFalse(map.UsedActionTypes.Contains(typeof(DummyAction5)));
        }

        [TestMethod]
        public void AddsParallelTypes()
        {
            map
                .StartsAt<DummyAction1>()
                .Then<DummyAction2>()
                .Then<DummyAction3>()
                .ThenParallely<DummyAction4, DummyAction5>();

            Assert.IsTrue(map.UsedActionTypes.Contains(typeof(DummyAction1)));
            Assert.IsTrue(map.UsedActionTypes.Contains(typeof(DummyAction2)));
            Assert.IsTrue(map.UsedActionTypes.Contains(typeof(DummyAction3)));
            Assert.IsTrue(map.UsedActionTypes.Contains(typeof(DummyAction4)));
            Assert.IsTrue(map.UsedActionTypes.Contains(typeof(DummyAction5)));
        }

        [TestMethod]
        public void HoldsCorrectPositionPositive()
        {
            map
                .StartsAt<DummyAction1>()
                .Then<DummyAction2>()
                .ThenParallely<DummyAction3, DummyAction4>();

            Assert.AreEqual(0, map.GetPosition(typeof(DummyAction1)));
            Assert.AreEqual(1, map.GetPosition(typeof(DummyAction2)));
            Assert.AreEqual(2, map.GetPosition(typeof(DummyAction3)));
            Assert.AreEqual(3, map.GetPosition(typeof(DummyAction4)));
        }

        [TestMethod]
        public void HoldsCorrectPositionNegative()
        {
            map
                .StartsAt<DummyAction1>()
                .Then<DummyAction2>()
                .ThenParallely<DummyAction3, DummyAction4>();

            Assert.AreEqual(-1, map.GetPosition(typeof(DummyAction5)));
        }

        [TestMethod]
        public void ComparesPositions()
        {
            map
                .StartsAt<DummyAction1>()
                .Then<DummyAction2>()
                .ThenParallely<DummyAction3, DummyAction4>();

            Assert.AreEqual(-1, map.ComparePosition(typeof(DummyAction1), typeof(DummyAction2)));
            Assert.AreEqual(-1, map.ComparePosition(typeof(DummyAction1), typeof(DummyAction3)));
            Assert.AreEqual(-1, map.ComparePosition(typeof(DummyAction3), typeof(DummyAction4)));
            Assert.AreEqual(1, map.ComparePosition(typeof(DummyAction4), typeof(DummyAction1)));
        }

        [TestMethod]
        public void ReturnsCorrectNextAction()
        {
            map
                .StartsAt<DummyAction1>()
                .Then<DummyAction2>()
                .ThenParallely<DummyAction3, DummyAction4>();

            Assert.AreEqual(1, map.GetNext(new IAction[0]).Count());
            Assert.AreEqual(typeof(DummyAction1), map.GetNext(new IAction[0]).First());
            Assert.AreEqual(1, map.GetNext(new[] { new DummyAction1()}).Count());
            Assert.AreEqual(typeof(DummyAction2), map.GetNext(new[] { new DummyAction1() }).First());
            Assert.AreEqual(2, map.GetNext(new[] { new DummyAction2() }).Count());
            Assert.AreEqual(typeof(DummyAction3), map.GetNext(new[] { new DummyAction2() }).First());
            Assert.AreEqual(typeof(DummyAction4), map.GetNext(new[] { new DummyAction2() }).Last());
            Assert.AreEqual(0, map.GetNext(new[] { new DummyAction3() }).Count());
            Assert.AreEqual(0, map.GetNext(new[] { new DummyAction4() }).Count());
        }

        [TestMethod]
        public void FailsOnMultipleUse()
        {
            map.StartsAt<DummyAction1>();

            Assert.ThrowsException<MultipleActionUseException> (() => map.Then<DummyAction1>());
        }

        class DummyAction : StartActionBase<NullObject>
        {
            public DummyAction() 
            {
            }

            public override NullObject Execute()
			{
				return NullObject.Value;
			}
        }

        class DummyAction1 : DummyAction { }
        class DummyAction2 : DummyAction { }
        class DummyAction3 : DummyAction { }
        class DummyAction4 : DummyAction { }
        class DummyAction5 : DummyAction { }
    }
}
