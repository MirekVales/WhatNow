using System;
using System.Collections.Generic;

namespace WhatNow.Contracts
{
    public abstract class TypeStore<StoreType> where StoreType : class
    {
        readonly object accessLock = new object();
        readonly Dictionary<Type, object> values = new Dictionary<Type, object>();

        public bool Contains<T>()
        {
            lock (accessLock)
                return values.ContainsKey(typeof(T));
        }

		public T Get<T>() => (T)Get(typeof(T));

		public object Get(Type type)
        {
            lock (accessLock)
                if (values.ContainsKey(type))
                    return values[type];

            throw new Exception($"Type {type.Name} was not found");
        }

        public bool TryGet<T>(out T value)
        {
            lock (accessLock)
                if (values.ContainsKey(typeof(T)))
                {
                    value = (T)values[typeof(T)];
                    return true;
                }

            value = default(T);
            return false;
        }

        public StoreType Do<T>(Action<T> operation)
        {
            lock (accessLock)
                operation(Get<T>());

            return this as StoreType;
        }

        public bool TryDo<T>(Action<T> operation)
        {
            lock (accessLock)
                if (TryGet(out T value))
                {
                    operation(value);
                    return true;
                }

            return false;
        }

        public StoreType Set(object value)
        {
            lock (accessLock)
                values[value.GetType()] = value;

            return this as StoreType;
        }
    }
}
