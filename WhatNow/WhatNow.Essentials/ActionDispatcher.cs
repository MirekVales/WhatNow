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
        readonly DedicatedThreadPoolTaskScheduler scheduler;
        readonly TaskFactory taskFactory;

        public IReadOnlyCollection<IActionPipe> Pipes => pipes.ToArray();

        public ActionDispatcher(IEnumerable<IActionPipe> actionPipes, int maxDegreeOfParallelism = 8)
        {
            pipes = actionPipes.ToArray();
            cancellationTokenSource = new CancellationTokenSource();
            tasks = new Dictionary<IActionPipe, Task>(pipes.Length);

            pool = new DedicatedThreadPool(new DedicatedThreadPoolSettings(maxDegreeOfParallelism));
            scheduler = new DedicatedThreadPoolTaskScheduler(pool);
            taskFactory = new TaskFactory(cancellationTokenSource.Token, TaskCreationOptions.None, TaskContinuationOptions.None, scheduler);
        }

        public ActionDispatcher(int maxDegreeOfParallelism = 8, params IActionPipe[] actionPipes)
        {
            pipes = actionPipes.ToArray();
            cancellationTokenSource = new CancellationTokenSource();
            tasks = new Dictionary<IActionPipe, Task>(pipes.Length);

            pool = new DedicatedThreadPool(new DedicatedThreadPoolSettings(maxDegreeOfParallelism));
            scheduler = new DedicatedThreadPoolTaskScheduler(pool);
            taskFactory = new TaskFactory(cancellationTokenSource.Token, TaskCreationOptions.None, TaskContinuationOptions.None, scheduler);
        }

        public void DoEvents()
        {
            foreach (var pipe in pipes)
            {
                if (pipe.BreakRequested)
                    continue;

                if (tasks.ContainsKey(pipe) && !tasks[pipe].IsCompleted)
                    continue;

                if (pipe.TryGetNextTask(taskFactory, out Task task))
                {
                    tasks[pipe] = task;
                }
            }
        }

        public async Task AsyncExecute()
        {
            if (IsFinished)
                throw new InvalidOperationException();

           await new Task(() => { while (!IsFinished) DoEvents(); });
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
