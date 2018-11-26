using Helios.Concurrency;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using WhatNow.Contracts.Resources;
using WhatNow.Contracts.Resources.Accessable;
using WhatNow.Contracts.ThreadPool;

namespace WhatNow.Essentials.ResourceTypes
{
    public class ThreadPool : IAccessableResource, IThreadPool
    {
        readonly DedicatedThreadPool threadPool;
        readonly CancellationTokenSource cancellationTokenSource;
        readonly TaskFactory taskFactory;
        readonly TaskScheduler taskScheduler;

        public string Name => threadPool.Settings.Name;
        public int Threads => threadPool.Settings.NumThreads;

        public ThreadPool(DedicatedThreadPool threadPool, TaskCreationOptions taskCreationOptions)
        {
            this.threadPool = threadPool;

            cancellationTokenSource = new CancellationTokenSource();
            taskScheduler = new DedicatedThreadPoolTaskScheduler(threadPool);
            taskFactory = new TaskFactory(
                cancellationTokenSource.Token
                , taskCreationOptions
                , TaskContinuationOptions.None
                , taskScheduler);
        }

        public void Invoke(params Action[] actions)
        {
            if (actions.Length == 0)
                return;

            using (var list = new TaskList(taskFactory))
            {
                list
                    .With(actions)
                    .WaitAllFinished();
            }
        }

        public ITaskList CreateTaskList()
            => new TaskList(taskFactory);

        public void ForEach<T>(IEnumerable<T> source, Action<T> body)
        {
            var options = new ParallelOptions()
            {
                TaskScheduler = taskScheduler
            };
            Parallel.ForEach(source, options, body);
        }

        public void ForEach<T>(IEnumerable<T> source, ParallelOptions parallelOptions, Action<T> body)
        {
            var options = new ParallelOptions()
            {
                CancellationToken = parallelOptions.CancellationToken
                , MaxDegreeOfParallelism = parallelOptions.MaxDegreeOfParallelism
                , TaskScheduler = taskScheduler
            };
            Parallel.ForEach(source, options, body);
        }

        public void Dispose()
        {
            cancellationTokenSource?.Dispose();
            threadPool?.Dispose();
        }
    }
}
