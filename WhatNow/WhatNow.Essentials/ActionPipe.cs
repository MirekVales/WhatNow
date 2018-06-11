using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WhatNow.Contracts;
using WhatNow.Contracts.Actions;
using WhatNow.Contracts.Dependency;

namespace WhatNow.Essentials
{
    public class ActionPipe : IActionPipe
    {
		readonly ActionToken actionToken;

		readonly IDependencyResolver dependencyResolver;

		Dictionary<Type, IAction> actions;

        public IAction[] Current { get; private set; }
        public IAction[] Next => Map
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

        public ActionPipe(IActionPipeMap map, ActionToken actionToken, IDependencyResolver dependencyResolver)
        {
			this.actionToken = actionToken;
			this.dependencyResolver = dependencyResolver;
			Map = map;

            actions = map
                .UsedActionTypes
                .ToDictionary(k => k, t => (IAction)dependencyResolver.Resolve(t));

            Current = new IAction[0];

            executions = actions.Keys.ToDictionary(k => k, v => new HashSet<TimeSpan>());
        }

        public void Restart()
        {
            if (!Finished && !BreakRequested)
                throw new InvalidOperationException();

            actions = Map
                .UsedActionTypes
				.ToDictionary(k => k, t => (IAction)dependencyResolver.Resolve(t));

			Current = new IAction[0];
        }

        public bool TryGetNextTask(TaskFactory taskFactory, out Task task)
        {
            task = null;

            if (!BreakRequested && FinishedCurrent)
            {
                Current = Next;

                if (!Current.Any())
                    return false;

                var tasks = Current.Select(c => taskFactory.StartNew(() =>
                {
					using (new BlockStopwatch(t => executions[c.GetType()].Add(t)))
					{
						ExecuteAction(c);
					}
                }));
                task = Task.WhenAll(tasks);
                return true;
            }
            return false;
        }

		void ExecuteAction(IAction action)
		{
			var inType = action.InputType;
			
			object GetValue(Type type) => 
				type == typeof(NullObject) ? NullObject.Value : actionToken.Get(type);

			object GetMultiple(Type[] types)
			{
				var values = types.Select(GetValue).ToArray();
				return Activator.CreateInstance(inType, values);
			}

			var inValue = inType.FullName.StartsWith("System.ValueTuple")
                ? GetMultiple(inType.GenericTypeArguments)
                : GetValue(inType);
			var outValue = action.ExecuteUntyped(inValue);
			if (!(outValue is NullObject))
			{
				actionToken.Set(outValue);
			}
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
