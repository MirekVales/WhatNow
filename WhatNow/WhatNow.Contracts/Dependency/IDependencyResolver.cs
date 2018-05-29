using System;

namespace WhatNow.Contracts.Dependency
{
	public interface IDependencyResolver
	{
		object Resolve(Type type);

		void Release(object instance);
	}
}