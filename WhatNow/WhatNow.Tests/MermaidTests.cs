using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using WhatNow.Contracts;
using WhatNow.Contracts.Actions;
using WhatNow.Essentials;
using WhatNow.Visualization.Mermaid;

namespace WhatNow.Tests
{
    [TestClass]
    public class MermaidTests
    {
        [TestMethod]
        public void VisualizesPipeMap()
        {
            var map = new ActionPipeMap()
                .StartsAt<DummyStart>()
                .Then<DummyAction1>()
                .ThenParallely<DummyAction2, DummyAction3, DummyAction4>()
                .Then<DummyAction5>()
                .Then<DummyAction6>()
                .ThenParallely<DummyAction7, DummyAction8, DummyAction9>()
                .Then<DummyTermination>();

            var pipeVisualization = new MermaidVisualizer().Visualize(map);

            Assert.IsTrue(pipeVisualization.Contains("graph TD"));
            Assert.IsTrue(pipeVisualization.Contains("subgraph DummyStart"));
            Assert.IsTrue(pipeVisualization.Contains("1[DummyStart] --> |IDisposable| 2[DummyAction1]"));
            Assert.IsTrue(pipeVisualization.Contains("2[DummyAction1] --> |NullObject| 3[DummyAction2]"));
            Assert.IsTrue(pipeVisualization.Contains("3[DummyAction2] --> |NullObject| 4[DummyAction5]"));
            Assert.IsTrue(pipeVisualization.Contains("4[DummyAction5] --> |NullObject| 5[DummyAction6]"));
            Assert.IsTrue(pipeVisualization.Contains("5[DummyAction6] --> |NullObject| 6[DummyAction7]"));
            Assert.IsTrue(pipeVisualization.Contains("6[DummyAction7] --> |NullObject| 7[DummyTermination]"));
            Assert.IsTrue(pipeVisualization.Contains("5[DummyAction6] --> |NullObject| 8[DummyAction8]"));
            Assert.IsTrue(pipeVisualization.Contains("5[DummyAction6] --> |NullObject| 9[DummyAction9]"));
            Assert.IsTrue(pipeVisualization.Contains("2[DummyAction1] --> |NullObject| 10[DummyAction3]"));
            Assert.IsTrue(pipeVisualization.Contains("2[DummyAction1] --> |NullObject| 11[DummyAction4]"));
            Assert.IsTrue(pipeVisualization.Contains("end"));
        }

        class DummyStart : StartActionBase<IDisposable>
        {
            public DummyStart()
            {
            }

            public override IDisposable Execute()
            {
                return null;
            }
        }

        class DummyAction1 : PipelineActionBase<NullObject, NullObject>
        {
            public DummyAction1()
            {
            }

            public override NullObject Execute(NullObject obj)
            {
                return NullObject.Value;
            }
        }

        class DummyAction2 : DummyAction1 { }
        class DummyAction3 : DummyAction1 { }
        class DummyAction4 : DummyAction1 { }
        class DummyAction5 : DummyAction1 { }
        class DummyAction6 : DummyAction1 { }
        class DummyAction7 : DummyAction1 { }
        class DummyAction8 : DummyAction1 { }
        class DummyAction9 : DummyAction1 { }
        class DummyTermination : TerminatorActionBase<IDisposable>
        {
            public DummyTermination()
            {
            }

            public override void Execute(IDisposable obj)
            {
            }
        }
    }
}
