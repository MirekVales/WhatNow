using Helios.Concurrency;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WhatNow.Contracts;

namespace WhatNow.Essentials
{
    public class ActionDispatcher : IActionDispatcher
    {
        readonly IActionPipe[] pipes;
        readonly CancellationTokenSource cancellationTokenSource;
        readonly Dictionary<IActionPipe, Task> tasks;

        readonly DedicatedThreadPool pool;
        public DedicatedThreadPoolTaskScheduler Scheduler { get; }
        public TaskFactory TaskFactory { get; }

        public IReadOnlyCollection<IActionPipe> Pipes => pipes.ToArray();

        public ActionDispatcher(IEnumerable<IActionPipe> actionPipes, int numberOfThreadPoolThreads = 8)
        {
            pipes = actionPipes.ToArray();
            cancellationTokenSource = new CancellationTokenSource();
            tasks = new Dictionary<IActionPipe, Task>(pipes.Length);

            pool = new DedicatedThreadPool(new DedicatedThreadPoolSettings(numberOfThreadPoolThreads));
            Scheduler = new DedicatedThreadPoolTaskScheduler(pool);
            TaskFactory = new TaskFactory(cancellationTokenSource.Token, TaskCreationOptions.None, TaskContinuationOptions.None, Scheduler);
        }

        public ActionDispatcher(
            int numberOfThreadPoolThreads = 8
            , TaskCreationOptions taskCreationOptions = TaskCreationOptions.None
            , params IActionPipe[] actionPipes)
        {
            pipes = actionPipes.ToArray();
            cancellationTokenSource = new CancellationTokenSource();
            tasks = new Dictionary<IActionPipe, Task>(pipes.Length);

            pool = new DedicatedThreadPool(new DedicatedThreadPoolSettings(numberOfThreadPoolThreads));
            Scheduler = new DedicatedThreadPoolTaskScheduler(pool);
            TaskFactory = new TaskFactory(cancellationTokenSource.Token, taskCreationOptions, TaskContinuationOptions.None, Scheduler);
        }

        public void DoEvents()
        {
            foreach (var pipe in pipes)
            {
                if (pipe.BreakRequested)
                    continue;

                if (tasks.ContainsKey(pipe) && !tasks[pipe].IsCompleted)
                    continue;

                if (pipe.TryGetNextTask(TaskFactory, out Task task))
                {
                    tasks[pipe] = task;
                }
            }

            if (IsFinished)
                Array.ForEach(pipes, p => p.ActionToken.ClearSingleRunValues());
        }

        public async Task AsyncExecute()
        {
            if (IsFinished)
                throw new InvalidOperationException();

            var observerTask = new Task(
                () => {
                    while (!IsFinished)
                        DoEvents();
                });
            observerTask.Start();
            await observerTask;
        }

        public bool IsFinished
            => pipes.All(p => p.Finished || p.BreakRequested);

        public bool EndedByBreak
            => pipes.Any(p => p.BreakRequested);

        public void Dispose()
        {
            cancellationTokenSource.Cancel();
            cancellationTokenSource.Dispose();
            pool.Dispose();
        }
    }
}
