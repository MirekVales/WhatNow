namespace WhatNow.Essentials
{
    using Helios.Concurrency;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using WhatNow.Contracts;
    using WhatNow.Contracts.Actions;
    using WhatNow.Contracts.Execution;
    using WhatNow.Contracts.Resources;
    using WhatNow.Essentials.Resources;

    public class ActionDispatcher : IActionDispatcher
    {
        readonly IActionPipe[] pipes;
        readonly CancellationTokenSource cancellationTokenSource;
        readonly Dictionary<IActionPipe, Task> tasks;

        readonly IResourceManager resourceManager;

        readonly DedicatedThreadPool pool;
        public DedicatedThreadPoolTaskScheduler Scheduler { get; }
        public TaskFactory TaskFactory { get; }

        public IReadOnlyCollection<IActionPipe> Pipes => pipes.ToArray();

        readonly object evaluationLock = new object();

        public delegate void PipeEvent();

        public event PipeEvent OnProcessingBegin;
        public event PipeEvent OnProcessingEnd;

        public DispatcherState State =>
            EvaluationFunction(() => state);
        DispatcherState state;

        public ActionDispatcher(IEnumerable<IActionPipe> actionPipes, int numberOfThreadPoolThreads = 8)
        {
            pipes = actionPipes.ToArray();
            resourceManager = new ResourceManager();

            cancellationTokenSource = new CancellationTokenSource();
            tasks = new Dictionary<IActionPipe, Task>(pipes.Length);

            pool = new DedicatedThreadPool(new DedicatedThreadPoolSettings(numberOfThreadPoolThreads));
            Scheduler = new DedicatedThreadPoolTaskScheduler(pool);
            TaskFactory = new TaskFactory(
                cancellationTokenSource.Token
                , TaskCreationOptions.None
                , TaskContinuationOptions.None, Scheduler);

            SetUpProcessingDefaultEvents();

            state = DispatcherState.NotStarted;
        }

        public ActionDispatcher(
            IResourcePlan resourcePlan
            , int numberOfThreadPoolThreads = 8
            , TaskCreationOptions taskCreationOptions = TaskCreationOptions.None
            , params IActionPipe[] actionPipes)
        {
            pipes = actionPipes.ToArray();
            resourceManager = new ResourceManager(resourcePlan);

            cancellationTokenSource = new CancellationTokenSource();
            tasks = new Dictionary<IActionPipe, Task>(pipes.Length);

            pool = new DedicatedThreadPool(new DedicatedThreadPoolSettings(numberOfThreadPoolThreads));
            Scheduler = new DedicatedThreadPoolTaskScheduler(pool);
            TaskFactory = new TaskFactory(
                cancellationTokenSource.Token
                , taskCreationOptions
                , TaskContinuationOptions.None
                , Scheduler);

            SetUpProcessingDefaultEvents();

            state = DispatcherState.NotStarted;
        }

        void SetUpProcessingDefaultEvents()
        {
            OnProcessingBegin += ActionDispatcher_OnProcessingBegin;
            OnProcessingEnd += ActionDispatcher_OnProccesingEnd;
        }

        void TearDownProcessingDefaultEvents()
        {
            OnProcessingBegin -= ActionDispatcher_OnProcessingBegin;
            OnProcessingEnd -= ActionDispatcher_OnProccesingEnd;
        }

        void ActionDispatcher_OnProccesingEnd()
        {
            Array.ForEach(pipes, p => p.ActionToken.ClearSingleRunValues());
            resourceManager.FreeResources();
            lock (evaluationLock)
                state = DispatcherState.Finished;
        }

        void ActionDispatcher_OnProcessingBegin()
        {
            resourceManager.AllocateResources();
            lock (evaluationLock)
                state = DispatcherState.Processing;
        }

        public void DoEvents()
        {
            if (State == DispatcherState.NotStarted && IsVeryStart)
            {
                OnProcessingBegin();
            }
            else if (State == DispatcherState.Processing)
            {
                foreach (var pipe in pipes)
                {
                    if (pipe.BreakRequested)
                        continue;

                    if (tasks.ContainsKey(pipe) && !tasks[pipe].IsCompleted)
                        continue;

                    if (pipe.TryGetNextTask(resourceManager, TaskFactory, out Task task))
                    {
                        tasks[pipe] = task;
                    }
                }

                if (pipesFinished)
                {
                    OnProcessingEnd();
                }
            }
        }

        public async Task AsyncExecute()
        {
            if (IsFinished)
                throw new InvalidOperationException();

            await TaskFactory.StartNew(() =>
            {
                while (!IsFinished)
                    DoEvents();
            });
        }

        public void Restart()
        {
            if (!IsFinished)
                throw new InvalidOperationException();

            lock (evaluationLock)
            {
                state = DispatcherState.Unspecified;
                foreach (var pipe in pipes)
                    pipe.Restart();
                state = DispatcherState.NotStarted;
            }
        }

        T EvaluationFunction<T>(Func<T> f)
        {
            lock (evaluationLock)
                return f();
        }

        public bool IsVeryStart
            => EvaluationFunction(() => pipes.All(p => p.Current.Length == 0) && !IsFinished);

        public bool IsFinished
            => EvaluationFunction(() => State == DispatcherState.Finished);

        bool pipesFinished
            => EvaluationFunction(() => pipes.All(p => p.Finished || p.BreakRequested));

        public bool EndedByBreak
            => EvaluationFunction(() => pipes.Any(p => p.BreakRequested));

        public void Dispose()
        {
            cancellationTokenSource.Cancel();
            cancellationTokenSource.Dispose();

            TearDownProcessingDefaultEvents();

            pool.Dispose();
        }
    }
}