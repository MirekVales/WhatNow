using System;
using System.Collections.Generic;

namespace WhatNow.Contracts
{
    public interface IActionDispatcher : IDisposable
    {
        bool EndedByBreak { get; }
        bool IsFinished { get; }

        void DoEvents();
        IEnumerable<BreakRequestReason> GetBreakReasons();
        IEnumerable<(IActionPipe,ProcessingStatistics)> ProcessingStats { get; }
    }
}