﻿using System;
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

        public IReadOnlyCollection<IActionPipe> Pipes => pipes.ToArray();

        public ActionDispatcher(IEnumerable<IActionPipe> actionPipes)
        {
            pipes = actionPipes.ToArray();
            cancellationTokenSource = new CancellationTokenSource();
            tasks = new Dictionary<IActionPipe, Task>(pipes.Length);
        }

        public ActionDispatcher(params IActionPipe[] actionPipes)
        {
            pipes = actionPipes.ToArray();
            cancellationTokenSource = new CancellationTokenSource();
            tasks = new Dictionary<IActionPipe, Task>(pipes.Length);
        }

        public void DoEvents()
        {
            foreach (var pipe in pipes)
            {
                if (pipe.BreakRequested)
                    continue;

                if (tasks.ContainsKey(pipe) && !tasks[pipe].IsCompleted)
                    continue;

                if (pipe.TryGetNextTask(cancellationTokenSource.Token, out Task task))
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

        public IEnumerable<(IActionPipe pipe, BreakRequestReason reason)> GetBreakReasons()
            => pipes.SelectMany(p => p.BreakReasons.Select(r => (p, r)));

        public IEnumerable<(IActionPipe pipe, ProcessingStatistics statistics)> ProcessingStats
            => pipes.SelectMany(p => p.ProcessingStats.Select(s => (p, s)));

        public void Dispose()
        {
            cancellationTokenSource.Cancel();
            cancellationTokenSource.Dispose();
        }
    }
}
