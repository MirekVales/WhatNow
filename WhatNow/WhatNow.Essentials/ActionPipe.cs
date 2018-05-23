using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WhatNow.Contracts;

namespace WhatNow.Essentials
{
    public class ActionPipe : IActionPipe
    {
        Dictionary<Type, ActionBase> actions;

        public ActionBase[] Current { get; private set; }
        public ActionBase[] Next => Map
            .GetNext(Current)
            .Select(t => actions[t])
            .ToArray();

        public IActionPipeMap Map { get; }

        public bool Finished => actions.All(a => a.Value.Finished);

        public bool FinishedCurrent => Current.All(a => a.Finished);

        public bool BreakRequested => Current.Any(a => a.BreakRequested);

        readonly Dictionary<Type, HashSet<TimeSpan>> executions;

        public IEnumerable<BreakRequestReason> BreakReasons => actions
            .Select(a => a.Value.BreakRequestReason)
            .Where(br => br != null)
            .ToArray();

        public ActionPipe(IActionPipeMap map, ActionToken actionToken, DependencyContainer dependencyContainer)
        {
            Map = map;

            actions = map
                .UsedActionTypes
                .ToDictionary(k => k, t => (ActionBase)Activator.CreateInstance(t, dependencyContainer, actionToken));

            Current = new ActionBase[0];

            executions = actions.Keys.ToDictionary(k => k, v => new HashSet<TimeSpan>());
        }

        public void Restart(ActionToken actionToken, DependencyContainer dependencyContainer)
        {
            if (!Finished && !BreakRequested)
                throw new InvalidOperationException();

            actions = Map
                .UsedActionTypes
                .ToDictionary(k => k, t => (ActionBase)Activator.CreateInstance(t, dependencyContainer, actionToken));

            Current = new ActionBase[0];
        }

        public bool TryGetNextTask(CancellationToken cancellationToken, out Task task)
        {
            task = null;

            if (!BreakRequested && FinishedCurrent)
            {
                Current = Next;

                if (!Current.Any())
                    return false;

                var tasks = Current.Select(c => Task.Run(() =>
                {
                    using (new BlockStopwatch(t => executions[c.GetType()].Add(t)))
                        c.ExecuteAction();
                }, cancellationToken));
                task = Task.WhenAll(tasks);
                return true;
            }
            return false;
        }

        public IEnumerable<ProcessingStatistics> ProcessingStats
            => GetProcessingStats();

        IEnumerable<ProcessingStatistics> GetProcessingStats()
        {
            foreach (var type in executions)
            {
                yield return new ProcessingStatistics(
                    type.Key,
                    Map.GetPosition(type.Key),
                    type.Value.Count,
                    type.Value.Any()
                    ? TimeSpan.FromMilliseconds(type.Value.Select(v => v.TotalMilliseconds).Average())
                    : TimeSpan.Zero);
            }
        }
    }
}
