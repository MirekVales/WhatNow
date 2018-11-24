using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using WhatNow.Contracts;
using WhatNow.Contracts.Actions;

namespace WhatNow.Essentials
{
    public class ActionPipeMap : IActionPipeMap
    {
        readonly HashSet<(Type End, Type Start)> map = new HashSet<(Type End, Type Start)>();
        readonly List<Type> types = new List<Type>();

        Type[] currentType = new Type[0];

        public ActionPipeMap StartsAt<T>()
            where T : IAction
        {
            currentType = new Type[] { typeof(T) };

            types.Add(typeof(T));
            return this;
        }

        public ActionPipeMap Then<T>()
            where T : IAction
        {
            if (types.Contains(typeof(T)))
                throw new MultipleActionUseException();

            foreach (var ctype in currentType)
                map.Add((typeof(T), ctype));

            currentType = new Type[] { typeof(T) };

            types.Add(typeof(T));

            return this;
        }

        public ActionPipeMap ThenParallely<T1, T2>()
            where T1 : IAction
            where T2 : IAction
        {
            if (types.Contains(typeof(T1)) || types.Contains(typeof(T2)))
                throw new MultipleActionUseException();

            foreach (var ctype in currentType)
            {
                map.Add((typeof(T1), ctype));
                map.Add((typeof(T2), ctype));
            }

            currentType = new Type[] { typeof(T1), typeof(T2) };
            types.Add(typeof(T1));
            types.Add(typeof(T2));

            return this;
        }

        public ActionPipeMap ThenParallely<T1, T2, T3>()
            where T1 : IAction
            where T2 : IAction
            where T3 : IAction
        {
            if (types.Contains(typeof(T1)) || types.Contains(typeof(T2)) || types.Contains(typeof(T3)))
                throw new MultipleActionUseException();

            foreach (var ctype in currentType)
            {
                map.Add((typeof(T1), ctype));
                map.Add((typeof(T2), ctype));
                map.Add((typeof(T3), ctype));
            }

            currentType = new Type[] { typeof(T1), typeof(T2), typeof(T3) };
            types.Add(typeof(T1));
            types.Add(typeof(T2));
            types.Add(typeof(T3));

            return this;
        }

        public ActionPipeMap ThenParallely<T1, T2, T3, T4>()
            where T1 : IAction
            where T2 : IAction
            where T3 : IAction
            where T4 : IAction
        {
            if (types.Contains(typeof(T1))
                || types.Contains(typeof(T2))
                || types.Contains(typeof(T3))
                || types.Contains(typeof(T4)))
                throw new MultipleActionUseException();

            foreach (var ctype in currentType)
            {
                map.Add((typeof(T1), ctype));
                map.Add((typeof(T2), ctype));
                map.Add((typeof(T3), ctype));
                map.Add((typeof(T4), ctype));
            }

            currentType = new Type[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4) };
            types.Add(typeof(T1));
            types.Add(typeof(T2));
            types.Add(typeof(T3));
            types.Add(typeof(T4));

            return this;
        }

        public IEnumerable<Type> GetEntryPoints()
            => map
            .Where(p => !map.Any(m => m.End == p.Start))
            .Select(m => m.Start);

        public IEnumerable<Type> GetNext(IEnumerable<IAction> currents)
            => GetNext(currents.Select(c => c.GetType()));

        public IEnumerable<Type> GetNext(IEnumerable<Type> currents)
        {
            if (!currents.Any())
                yield return types.Except(map.Select(m => m.End)).Single();
            else
                foreach (var next in currents.SelectMany(GetNext).Distinct())
                    yield return next;
        }

        public IEnumerable<Type> GetNext(Type current)
            => map
                .Where(p => p.Start == current)
                .Select(p => p.End)
                .ToArray();

        public int ComparePosition(Type t1, Type t2)
        {
            var index1 = types.IndexOf(t1);
            var index2 = types.IndexOf(t2);

            if (index1 + index2 == -2)
                throw new Exception();

            if (index1 < index2)
                return -1;

            if (index1 > index2)
                return 1;

            return 0;
        }

        public int GetPosition(Type t)
            => types.IndexOf(t);

        public IEnumerable<Type> UsedActionTypes => types.ToArray();

        public int MaxDegreeOfParallelism
            => types.Max(t => map.Count(m => m.Start == t));

        public int GetDegreeOfParallelism(Type t)
        {
            var parent = map.FirstOrDefault(m => m.End == t);
            if (parent.Equals(default((Type End, Type Start))))
                return 1;

            return map.Count(m => m.Start == parent.End);
        }

        public IEnumerator<Type[]> GetEnumerator()
        {
            var nextPoints = GetEntryPoints();
            do
            {
                yield return nextPoints.ToArray();
            }
            while ((nextPoints = GetNext(nextPoints)).Any());
        }

        IEnumerator IEnumerable.GetEnumerator()
           => GetEnumerator();
    }
}
