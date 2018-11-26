using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WhatNow.Contracts.ThreadPool;

namespace WhatNow.Contracts.Resources.Accessable
{
    public interface IThreadPool : IAccessableResource
    {
        string Name { get; }
        int Threads { get; }

        ITaskList CreateTaskList();
        void ForEach<T>(IEnumerable<T> source, Action<T> body);
        void ForEach<T>(IEnumerable<T> source, ParallelOptions parallelOptions, Action<T> body);
        void Invoke(params Action[] actions);
    }
}