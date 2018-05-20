using System;
using System.Collections.Generic;

namespace WhatNow.Contracts
{
    public interface IActionDispatcher : IDisposable
    {
        bool EndedByBreak { get; }
        bool IsFinished { get; }

        void DoEvents();
        IEnumerable<(IActionPipe, BreakRequestReason)> GetBreakReasons();
        IEnumerable<(IActionPipe,ProcessingStatistics)> ProcessingStats { get; }
    }
}