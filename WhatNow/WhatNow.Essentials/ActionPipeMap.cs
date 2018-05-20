using System;
using System.Collections.Generic;
using System.Linq;
using WhatNow.Contracts;

namespace WhatNow.Essentials
{
    public class ActionPipeMap : IActionPipeMap
    {
        readonly HashSet<Tuple<Type, Type>> map = new HashSet<Tuple<Type, Type>>();
        readonly List<Type> types = new List<Type>();

        Type[] currentType = new Type[0];

        public ActionPipeMap StartsAt<T>()
            where T : ActionBase
        {
            currentType = new Type[] { typeof(T) };

            types.Add(typeof(T));
            return this;
        }

        public ActionPipeMap Then<T>()
            where T : ActionBase
        {
            if (types.Contains(typeof(T)))
                throw new Exception("This action is already used");

            foreach (var ctype in currentType)
                map.Add(Tuple.Create(typeof(T), ctype));

            currentType = new Type[] { typeof(T) };

            types.Add(typeof(T));

            return this;
        }

        public ActionPipeMap ThenParallely<T1, T2>()
            where T1 : ActionBase
            where T2 : ActionBase
        {
            if (types.Contains(typeof(T1)) || types.Contains(typeof(T2)))
                throw new Exception("This action is already used");

            foreach (var ctype in currentType)
            {
                map.Add(Tuple.Create(typeof(T1), ctype));
                map.Add(Tuple.Create(typeof(T2), ctype));
            }

            currentType = new Type[] { typeof(T1), typeof(T2) };
            types.Add(typeof(T1));
            types.Add(typeof(T2));

            return this;
        }

        public ActionPipeMap ThenParallely<T1, T2, T3>()
            where T1 : ActionBase
            where T2 : ActionBase
            where T3 : ActionBase
        {
            if (types.Contains(typeof(T1)) || types.Contains(typeof(T2)) || types.Contains(typeof(T3)))
                throw new Exception("This action is already used");

            foreach (var ctype in currentType)
            {
                map.Add(Tuple.Create(typeof(T1), ctype));
                map.Add(Tuple.Create(typeof(T2), ctype));
                map.Add(Tuple.Create(typeof(T3), ctype));
            }

            currentType = new Type[] { typeof(T1), typeof(T2), typeof(T3) };
            types.Add(typeof(T1));
            types.Add(typeof(T2));
            types.Add(typeof(T3));

            return this;
        }

        public IEnumerable<Type> GetNext(IEnumerable<ActionBase> currents)
        {
            if (!currents.Any())
                yield return types.Except(map.Select(m => m.Item1)).Single();
            else
                foreach (var next in currents.SelectMany(GetNext).Distinct())
                    yield return next;
        }

        IEnumerable<Type> GetNext(ActionBase current)
            => map
                .Where(p => p.Item2 == current.GetType())
                .Select(p => p.Item1)
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
    }
}
