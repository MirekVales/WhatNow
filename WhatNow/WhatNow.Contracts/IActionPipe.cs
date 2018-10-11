using System.Collections.Generic;
using System.Threading.Tasks;
using WhatNow.Contracts.Actions;

namespace WhatNow.Contracts
{
    public interface IActionPipe
    {
        IActionPipeMap Map { get; }
        bool BreakRequested { get; }
		IAction[] Current { get; }
        bool Finished { get; }
        bool FinishedCurrent { get; }
		IAction[] Next { get; }
        IEnumerable<BreakRequestReason> BreakReasons { get; }
        IEnumerable<ProcessingStatisticsItem> ProcessingStats { get; }

        bool TryGetNextTask(TaskFactory taskFactory, out Task task);

        void Restart();
    }
}