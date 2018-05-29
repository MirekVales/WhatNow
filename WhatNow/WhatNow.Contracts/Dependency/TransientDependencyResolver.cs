using System;

namespace WhatNow.Contracts.Dependency
{
	public class TransientDependencyResolver : IDependencyResolver
	{
		public object Resolve(Type type) => Activator.CreateInstance(type);

		public void Release(object instance)
		{
		}
	}
}