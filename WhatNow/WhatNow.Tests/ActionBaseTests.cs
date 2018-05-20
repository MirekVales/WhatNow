using Microsoft.VisualStudio.TestTools.UnitTesting;
using WhatNow.Contracts;

namespace WhatNow.Tests
{
    [TestClass]
    public class ActionBaseTests
    {
        readonly ActionToken token;
        readonly DummyAction action;
        readonly DummyAction action2;

        public ActionBaseTests()
        {
            var container = new DependencyContainer();
            token = new ActionToken();
            action = new DummyAction(false, container, token);
            action2 = new DummyAction(true, container, token);
        }

        [TestMethod]
        public void ExecutesAction()
        {
            Assert.IsFalse(action.Finished);
            Assert.IsFalse(action.BreakRequested);
            action.ExecuteAction();
            Assert.AreEqual("Finished", token.Get<string>());
            Assert.IsTrue(action.Finished);
            Assert.IsFalse(action.BreakRequested);
        }

        [TestMethod]
        public void BreaksAction()
        {
            Assert.IsFalse(action2.Finished);
            Assert.IsFalse(action2.BreakRequested);
            action2.ExecuteAction();
            Assert.AreEqual("BreakRequested", token.Get<string>());
            Assert.IsTrue(action2.Finished);
            Assert.IsTrue(action2.BreakRequested);
        }

        private class DummyAction : ActionBase
        {
            readonly bool requestBreak;

            public DummyAction(bool requestBreak, DependencyContainer container, ActionToken token)
                : base(container, token)
            {
                this.requestBreak = requestBreak;
            }

            protected override void Execute()
            {
                if (requestBreak)
                {
                    RequestBreak(new BreakRequestCustom(this.GetType(), "", null));
                    Token.Set("BreakRequested");
                }
                else
                    Token.Set("Finished");
            }
        }
    }
}
