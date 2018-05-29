using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WhatNow.Contracts;
using WhatNow.Contracts.Actions;
using WhatNow.Contracts.Dependency;
using WhatNow.Essentials;

namespace WhatNow.Tests
{
	[TestClass]
	public class ActionPipeTests
	{
		readonly ActionPipeMap map;

		readonly TaskFactory taskFactory;

		readonly ActionToken token;

		public ActionPipeTests()
		{
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
			Assert.IsTrue(pipe.FinishedCurrent);
			Assert.AreEqual(0, pipe.Current.Length);

			Assert.IsTrue(pipe.TryGetNextTask(taskFactory, out Task t1));
			t1.Wait();
			Assert.AreEqual(1, pipe.Current.Length);
			Assert.AreEqual(typeof(DummyAction1), pipe.Current[0].GetType());
			Assert.IsTrue(pipe.TryGetNextTask(taskFactory, out Task t2));
			t2.Wait();
			Assert.AreEqual(1, pipe.Current.Length);
			Assert.AreEqual(typeof(DummyAction2), pipe.Current[0].GetType());
			Assert.IsTrue(pipe.TryGetNextTask(taskFactory, out Task t3));
			t3.Wait();
			Assert.AreEqual(2, pipe.Current.Length);
			Assert.AreEqual(typeof(DummyAction3), pipe.Current[0].GetType());
			Assert.AreEqual(typeof(DummyAction4), pipe.Current[1].GetType());

			Assert.IsFalse(pipe.BreakRequested);
			Assert.IsTrue(pipe.Finished);
			Assert.IsTrue(pipe.FinishedCurrent);
			Assert.IsFalse(pipe.TryGetNextTask(taskFactory, out Task _));
			Assert.AreEqual(0, pipe.Current.Length);

			Assert.AreEqual(4, token.Get<DummyType>().Property);
			Assert.AreEqual(4, pipe.ProcessingStats.Count());
		}

		class DummyAction1 : StartActionBase<DummyType>
		{
			public override DummyType Execute() => new DummyType { Property = 1 };
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

		class DummyType
		{
			public int Property { get; set; }
		}
	}
}