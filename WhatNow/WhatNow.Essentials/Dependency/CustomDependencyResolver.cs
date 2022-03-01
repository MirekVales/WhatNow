namespace WhatNow.Essentials.Dependency
{
    using System;
    using WhatNow.Contracts.Dependency;

    public class CustomDependencyResolver : IDependencyResolver
    {
        readonly Func<Type, object> resolve;
        readonly Action<object> release;

        public CustomDependencyResolver(Func<Type, object> resolve, Action<object> release)
        {
            this.resolve = resolve;
            this.release = release;
        }

        public void Release(object instance)
            => release(instance);

        public object Resolve(Type type)
            => resolve(type);

        public T Resolve<T>() where T : class
            => resolve(typeof(T)) as T;
    }
}
