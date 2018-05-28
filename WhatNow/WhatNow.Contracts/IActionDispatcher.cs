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
    }
}