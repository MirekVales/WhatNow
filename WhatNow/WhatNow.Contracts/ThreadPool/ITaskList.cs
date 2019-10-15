using System;
using System.Collections.Generic;
using System.Threading;

namespace WhatNow.Contracts.ThreadPool
{
    public interface ITaskList : IDisposable
    {
        int TasksCount { get; }

        void Cancel();
        void WaitAllFinished();
        ITaskList With(Action action);
        ITaskList With(IEnumerable<Action> actions);
        ITaskList With(Action<CancellationToken> action);
    }
}
