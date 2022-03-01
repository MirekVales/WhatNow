namespace WhatNow.Tests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;
    using System.Data;
    using System.Linq;
    using System.Threading.Tasks;
    using WhatNow.Contracts.Actions;
    using WhatNow.Contracts.Data;
    using WhatNow.Contracts.Resources;
    using WhatNow.Essentials;
    using WhatNow.Essentials.Dependency;
    using WhatNow.Essentials.Resources;

    [TestClass]
    public class ActionPipeTests
    {
        ActionPipeMap map;
        TaskFactory taskFactory;
        ActionToken token;
        IResourceManager resourceManager;

        [TestInitialize]
        public void Init()
        {
            resourceManager = new ResourceManager();
            map = new ActionPipeMap()
                .StartsAt<DummyAction1>()
                .Then<DummyAction2>()
                .ThenParallely<DummyAction3, DummyAction4>();
            token = new ActionToken();
            taskFactory = new TaskFactory();
        }


        [TestMethod]
        public void ProcessesActions()
        {
            var pipe = new ActionPipe(map, token, new TransientDependencyResolver());

            Assert.IsFalse(pipe.BreakRequested);
            Assert.IsFalse(pipe.Finished);
            Assert.IsTrue(pipe.CurrentActionsFinished);
            Assert.AreEqual(0, pipe.Current.Length);
            Assert.AreEqual(2, map.MaxDegreeOfParallelism);

            Assert.IsTrue(pipe.TryGetNextTask(resourceManager, taskFactory, out Task t1));
            t1.Wait();
            Assert.AreEqual(1, pipe.Current.Length);
            Assert.AreEqual(typeof(DummyAction1), pipe.Current[0].GetType());
            Assert.IsTrue(pipe.TryGetNextTask(resourceManager, taskFactory, out Task t2));
            t2.Wait();
            Assert.AreEqual(1, pipe.Current.Length);
            Assert.AreEqual(typeof(DummyAction2), pipe.Current[0].GetType());
            Assert.IsTrue(pipe.TryGetNextTask(resourceManager, taskFactory, out Task t3));
            t3.Wait();
            Assert.AreEqual(2, pipe.Current.Length);
            Assert.AreEqual(typeof(DummyAction3), pipe.Current[0].GetType());
            Assert.AreEqual(typeof(DummyAction4), pipe.Current[1].GetType());

            Assert.IsFalse(pipe.BreakRequested);
            Assert.IsTrue(pipe.Finished);
            Assert.IsTrue(pipe.CurrentActionsFinished);
            Assert.IsFalse(pipe.TryGetNextTask(resourceManager, taskFactory, out Task _));
            Assert.AreEqual(0, pipe.Current.Length);

            Assert.AreEqual(4, token.Get<DummyType>().Property);
            Assert.AreEqual(4, pipe.ProcessingStats.Items.Count());
        }

        [TestMethod]
        public void MultipleDependencies()
        {
            var localMap = new ActionPipeMap()
                .StartsAt<DummyAction1>()
                .Then<DummyAction1B>()
                .Then<DummyMultipleIn>();

            var pipe = new ActionPipe(localMap, token, new TransientDependencyResolver());
            Assert.IsTrue(pipe.TryGetNextTask(resourceManager, taskFactory, out Task t1));
            t1.Wait();
            Assert.IsTrue(token.Contains<DummyType>());

            Assert.IsTrue(pipe.TryGetNextTask(resourceManager, taskFactory, out Task t2));
            t2.Wait();
            Assert.IsTrue(token.Contains<DummyType2>());

            Assert.IsTrue(pipe.TryGetNextTask(resourceManager, taskFactory, out Task t3));
            t3.Wait();

            Assert.IsFalse(pipe.BreakRequested);
            Assert.IsTrue(pipe.Finished);
            Assert.IsTrue(pipe.CurrentActionsFinished);
            Assert.IsFalse(pipe.TryGetNextTask(resourceManager, taskFactory, out Task _));
            Assert.AreEqual(0, pipe.Current.Length);
            Assert.AreEqual("ok", token.Get<string>());
        }

        [TestMethod]
        public void DisposesDisposableAndKeepsPermanentTokens()
        {
            var localMap = new ActionPipeMap()
                .StartsAt<DummyDisposableAction1>()
                .Then<DummyDisposableAction2>();

            var newToken = new ActionToken();
            var disposableType1 = new DummyDisposableType1();
            var disposableType2 = new DummyDisposableType2();
            newToken.Set(disposableType1, ItemLifespan.Permanent);
            newToken.Set(disposableType2, ItemLifespan.SingleRun);
            newToken.Set(new DummyType(), ItemLifespan.Permanent);
            newToken.Set(new DummyType2(), ItemLifespan.SingleRun);

            var pipe = new ActionPipe(localMap, newToken, new TransientDependencyResolver());
            using (var dispatcher = new ActionDispatcher(ResourcePlan.Empty, 2, TaskCreationOptions.None, pipe))
            {
                dispatcher.AsyncExecute().Wait();
            }

            Assert.IsFalse(newToken.Contains<IDisposable>());
            Assert.IsTrue(newToken.Contains<DummyDisposableType1>());
            Assert.IsFalse(newToken.Contains<DummyDisposableType2>());
            Assert.IsFalse(disposableType1.DisposeCalled);
            Assert.IsTrue(disposableType2.DisposeCalled);
            Assert.IsTrue(newToken.Contains<DummyType>());
            Assert.IsFalse(newToken.Contains<DummyType2>());
        }

        class DummyAction1 : StartActionBase<DummyType>
        {
            public override DummyType Execute() => new DummyType { Property = 1 };
        }

        class DummyAction1B : StartActionBase<DummyType2>
        {
            public override DummyType2 Execute() => new DummyType2 { Property = 1 };
        }

        class DummyAction2 : PipelineActionBase<DummyType, DummyType>
        {
            public override DummyType Execute(DummyType input)
            {
                input.Property++;
                return input;
            }
        }

        class DummyAction3 : PipelineActionBase<DummyType, DummyType>
        {
            public override DummyType Execute(DummyType input)
            {
                input.Property++;
                return input;
            }
        }

        class DummyAction4 : PipelineActionBase<DummyType, DummyType>
        {
            public override DummyType Execute(DummyType input)
            {
                input.Property++;
                return input;
            }
        }

        class DummyMultipleIn : PipelineActionBase<(DummyType, DummyType2), string>
        {
            public override string Execute((DummyType, DummyType2) input)
            {
                if (input.Item1 == null)
                    return "1 is null";
                if (input.Item2 == null)
                    return "2 is null";
                return "ok";
            }
        }

        class DummyType
        {
            public int Property { get; set; }
        }
        class DummyType2
        {
            public int Property { get; set; }
        }

        class DummyDisposableAction1 : StartActionBase<IDisposable>
        {
            public override IDisposable Execute() => new DataTable();
        }

        class DummyDisposableAction2 : TerminatorActionBase<IDisposable>
        {
            public override void Execute(IDisposable input)
            {
            }
        }

        class DummyDisposableType1 : IDisposable
        {
            public bool DisposeCalled { get; private set; }

            public void Dispose()
            {
                DisposeCalled = true;
            }
        }

        class DummyDisposableType2 : DummyDisposableType1
        {
        }
    }
}