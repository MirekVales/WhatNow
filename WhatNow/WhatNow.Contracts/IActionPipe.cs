using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace WhatNow.Contracts
{
    public interface IActionPipe
    {
        IActionPipeMap Map { get; }
        bool BreakRequested { get; }
        ActionBase[] Current { get; }
        bool Finished { get; }
        bool FinishedCurrent { get; }
        ActionBase[] Next { get; }
        IEnumerable<BreakRequestReason> BreakReasons { get; }
        IEnumerable<ProcessingStatistics> ProcessingStats { get; }

        bool TryGetNextTask(CancellationToken cancellationToken, out Task task);

        void Restart(ActionToken actionToken, DependencyContainer dependencyContainer);
    }
}