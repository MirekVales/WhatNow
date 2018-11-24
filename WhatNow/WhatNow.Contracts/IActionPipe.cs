using System.Collections.Generic;
using System.Threading.Tasks;
using WhatNow.Contracts.Actions;
using WhatNow.Contracts.Data;
using WhatNow.Contracts.Resources;

namespace WhatNow.Contracts
{
    public interface IActionPipe
    {
        ActionToken ActionToken { get; }
        IActionPipeMap Map { get; }
        bool BreakRequested { get; }
		IAction[] Current { get; }
        bool Finished { get; }
        bool CurrentActionsFinished { get; }
		IAction[] Next { get; }
        IEnumerable<BreakRequestReason> BreakReasons { get; }
        ProcessingStatistics ProcessingStats { get; }

        bool TryGetNextTask(IResourceManager resourceManager, TaskFactory taskFactory, out Task task);

        void Restart();
    }
}