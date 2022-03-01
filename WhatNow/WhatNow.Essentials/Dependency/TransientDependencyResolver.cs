namespace WhatNow.Essentials.Dependency
{
	using System;
	using WhatNow.Contracts.Dependency;

	public class TransientDependencyResolver : IDependencyResolver
	{
		public object Resolve(Type type) => Activator.CreateInstance(type);

		public void Release(object instance)
		{
		}
	}
}