using System;
using System.Collections.Generic;

namespace WhatNow.Contracts
{
    public interface IActionDispatcher : IDisposable
    {
        IReadOnlyCollection<IActionPipe> Pipes { get; }
        bool EndedByBreak { get; }
        bool IsFinished { get; }
        void DoEvents();
        IEnumerable<(IActionPipe pipe, BreakRequestReason reason)> GetBreakReasons();
        IEnumerable<(IActionPipe pipe, ProcessingStatistics statistics)> ProcessingStats { get; }
    }
}