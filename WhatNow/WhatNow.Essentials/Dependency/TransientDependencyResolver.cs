using System;
using WhatNow.Contracts.Dependency;

namespace WhatNow.Essentials.Dependency
{
	public class TransientDependencyResolver : IDependencyResolver
	{
		public object Resolve(Type type) => Activator.CreateInstance(type);

		public void Release(object instance)
		{
		}
	}
}