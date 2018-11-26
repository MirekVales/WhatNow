using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Helios.Concurrency;
using WhatNow.Contracts.ThreadPool;

namespace WhatNow.Essentials
{
    public class TaskList : ITaskList
    {
        readonly CancellationTokenSource cancellationTokenSource;
        readonly List<Task> tasks;

        readonly DedicatedThreadPool pool;
        readonly DedicatedThreadPoolTaskScheduler scheduler;
        readonly TaskFactory taskFactory;
        readonly object disposeLock = new object();

        bool disposed;

        public int TasksCount
            => tasks.Count;

        public TaskList(int maxDegreeOfParallelism)
        {
            pool = new DedicatedThreadPool(new DedicatedThreadPoolSettings(maxDegreeOfParallelism));
            scheduler = new DedicatedThreadPoolTaskScheduler(pool);
            taskFactory = new TaskFactory(scheduler);

            cancellationTokenSource = new CancellationTokenSource();
            tasks = new List<Task>();
        }

        public TaskList(TaskFactory taskFactory)
        {
            pool = null;
            scheduler = null;
            this.taskFactory = taskFactory;

            cancellationTokenSource = new CancellationTokenSource();
            tasks = new List<Task>();
        }

        public ITaskList With(Action action)
        {
            ThrowIfDisposed();

            tasks.Add(taskFactory.StartNew(action, cancellationTokenSource.Token));
            return this;
        }

        public ITaskList With(IEnumerable<Action> actions)
        {
            ThrowIfDisposed();

            foreach (var action in actions)
                tasks.Add(taskFactory.StartNew(action, cancellationTokenSource.Token));

            return this;
        }

        public void Cancel()
        {
            ThrowIfDisposed();

            cancellationTokenSource?.Dispose();
            pool?.Dispose();
        }

        public void WaitAllFinished()
        {
            ThrowIfDisposed();

            Task.WaitAll(tasks.ToArray());

            ClearTasks();
        }

        void ThrowIfDisposed()
        {
            lock (disposeLock)
                if (disposed)
                    throw new ObjectDisposedException(nameof(TaskList), $"Cannot use a disposed {nameof(TaskList)}");
        }

        public void Dispose()
        {
            lock (disposeLock)
            {
                if (disposed)
                    return;

                cancellationTokenSource?.Dispose();
                pool?.Dispose();

                ClearTasks();

                disposed = true;
            }
        }

        void ClearTasks()
        {
            foreach (var task in tasks)
                task.Dispose();

            tasks.Clear();
        }
    }
}
