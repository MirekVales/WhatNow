using System;

namespace WhatNow.Contracts
{
	public interface IDependencyResolver
	{
		object Resolve(Type type);

		void Release(object instance);
	}
}