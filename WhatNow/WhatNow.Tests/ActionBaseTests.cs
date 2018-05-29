using Microsoft.VisualStudio.TestTools.UnitTesting;
using WhatNow.Contracts;
using WhatNow.Contracts.Actions;

namespace WhatNow.Tests
{
    [TestClass]
    public class ActionBaseTests
    {
        readonly DummyAction action;
        readonly DummyAction action2;

        public ActionBaseTests()
        {
            action = new DummyAction(false);
            action2 = new DummyAction(true);
        }

        [TestMethod]
        public void ExecutesAction()
        {
            Assert.IsFalse(action.Finished);
            Assert.IsFalse(action.BreakRequested);
			var result = action.ExecuteUntyped(NullObject.Value);
            Assert.AreEqual("Finished", result);
            Assert.IsTrue(action.Finished);
            Assert.IsFalse(action.BreakRequested);
        }

        [TestMethod]
        public void BreaksAction()
        {
            Assert.IsFalse(action2.Finished);
            Assert.IsFalse(action2.BreakRequested);
			var result = action2.ExecuteUntyped(NullObject.Value);
			Assert.AreEqual("BreakRequested", result);
            Assert.IsTrue(action2.Finished);
            Assert.IsTrue(action2.BreakRequested);
        }

        private class DummyAction : StartActionBase<string>
        {
            readonly bool requestBreak;

            public DummyAction(bool requestBreak)
            {
                this.requestBreak = requestBreak;
            }

			public override string Execute()
			{
				if (requestBreak)
				{
					RequestBreak(new BreakRequestCustom(this.GetType(), "", null));
					return "BreakRequested";
				}
				else
					return "Finished";

			}
		}
    }
}
