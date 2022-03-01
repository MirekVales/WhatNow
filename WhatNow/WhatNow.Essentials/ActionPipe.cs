namespace WhatNow.Essentials
{
    using System;
    using System.Linq;
    using WhatNow.Contracts.Data;
    using System.Threading.Tasks;
    using WhatNow.Contracts.Actions;
    using System.Collections.Generic;
    using WhatNow.Contracts.Dependency;
    using WhatNow.Contracts.Resources;
    using WhatNow.Contracts.Statistics;

    public class ActionPipe : IActionPipe
    {
        public ActionToken ActionToken { get; }

        readonly IDependencyResolver dependencyResolver;

        Dictionary<Type, IAction> actions;

        public IAction[] Current { get; private set; }
        public IAction[] Next => Map
            .GetNext(Current)
            .Select(t => actions[t])
            .ToArray();

        public IActionPipeMap Map { get; }

        public bool Finished => actions.All(a => a.Value.Finished);
        public bool CurrentActionsFinished => Current.All(a => a.Finished);
        public bool BreakRequested => Current.Any(a => a.BreakRequested);

        readonly object executionsLock = new object();
        readonly Dictionary<Type, FixedSizedQueue<TimeSpan>> executions;

        public IEnumerable<BreakRequestReason> BreakReasons => actions
            .Select(a => a.Value.BreakRequestReason)
            .Where(br => br != null)
            .ToArray();

        public const int MAX_QUEUE_SIZE = 100;

        public ActionPipe(IActionPipeMap map, ActionToken actionToken, IDependencyResolver dependencyResolver)
        {
            ActionToken = actionToken;
            this.dependencyResolver = dependencyResolver;
            Map = map;

            actions = map
                .UsedActionTypes
                .ToDictionary(k => k, t => (IAction)dependencyResolver.Resolve(t));

            Current = new IAction[0];

            executions = actions.Keys.ToDictionary(k => k, _ => new FixedSizedQueue<TimeSpan>(MAX_QUEUE_SIZE));
        }

        public void Restart()
        {
            if (!Finished && !BreakRequested)
                throw new InvalidOperationException();

            actions = Map
                .UsedActionTypes
                .ToDictionary(k => k, t => (IAction)dependencyResolver.Resolve(t));

            Current = new IAction[0];

            lock (executionsLock)
                foreach (var type in executions.Values)
                    type.Clear();
        }

        public bool TryGetNextTask(IResourceManager resourceManager, TaskFactory taskFactory, out Task task)
        {
            task = null;

            if (!BreakRequested && CurrentActionsFinished)
            {
                Current = Next;

                if (!Current.Any())
                    return false;

                var tasks = Current.Select(c => taskFactory.StartNew(() =>
                {
                    using (new BlockStopwatch(t => WriteExecutionTime(c.GetType(), t)))
                    {
                        ExecuteAction(resourceManager, c);
                    }
                }));
                task = Task.WhenAll(tasks);
                return true;
            }
            return false;
        }

        void WriteExecutionTime(Type type, TimeSpan time)
        {
            lock (executionsLock)
                executions[type].Enqueue(time);
        }

        void ExecuteAction(IResourceManager resourceManager, IAction action)
        {
            var inType = action.InputType;

            object GetValue(Type type) =>
                type == typeof(NullObject) ? NullObject.Value : ActionToken.Get(type);

            object GetMultiple(Type[] types)
            {
                var values = types.Select(GetValue).ToArray();
                return Activator.CreateInstance(inType, values);
            }

            var inValue = inType.FullName.StartsWith("System.ValueTuple")
                ? GetMultiple(inType.GenericTypeArguments)
                : GetValue(inType);
            var outValue = action.ExecuteUntyped(resourceManager, inValue);
            if (!(outValue is NullObject))
            {
                ActionToken.Set(outValue, action.OutputType, ItemLifespan.SingleRun);
            }
        }

        public ProcessingStatistics ProcessingStats
            => new ProcessingStatistics(GetProcessingStats());

        IEnumerable<ProcessingStatisticsItem> GetProcessingStats()
        {
            lock (executionsLock)
                foreach (var type in executions)
                {
                    yield return new ProcessingStatisticsItem(
                        type.Key,
                        Map.GetPosition(type.Key),
                        type.Value.Count,
                        TimeSpan.FromMilliseconds(
                            type
                            .Value
                            .Select(v => v.TotalMilliseconds)
                            .DefaultIfEmpty(0)
                            .Average())
                        );
                }
        }
    }
}
