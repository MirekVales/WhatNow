using System;
using System.Collections.Generic;
using System.Linq;

namespace WhatNow.Contracts.Data
{
    public abstract class TypeStore<StoreType> where StoreType : class
    {
        readonly object accessLock = new object();
        readonly Dictionary<Type, object> values = new Dictionary<Type, object>();
        readonly Dictionary<Type, ItemLifespan> lifespans = new Dictionary<Type, ItemLifespan>();

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

            value = default;
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

        public StoreType Set(object value, ItemLifespan lifespan)
        {
            var type = value.GetType();
            lock (accessLock)
            {
                values[type] = value;
                lifespans[type] = lifespan;
            }

            return this as StoreType;
        }

        public StoreType Set(object value, Type boundedType, ItemLifespan lifespan)
        {
            var type = boundedType;
            lock (accessLock)
            {
                values[type] = value;
                lifespans[type] = lifespan;
            }

            return this as StoreType;
        }

        public StoreType Remove(Type type)
        {
            lock (accessLock)
                if (values.ContainsKey(type))
                {
                    values.Remove(type);
                    lifespans.Remove(type);
                }

            return this as StoreType;
        }
        
        public void ClearSingleRunValues()
        {
            lock (accessLock)
            {
                foreach (var pair in lifespans.ToArray())
                {
                    if (pair.Value == ItemLifespan.SingleRun)
                    {
                        if (typeof(IDisposable).IsAssignableFrom(pair.Key))
                            ((IDisposable)Get(pair.Key)).Dispose();

                        Remove(pair.Key);
                    }
                }
            }
        }
    }
}
