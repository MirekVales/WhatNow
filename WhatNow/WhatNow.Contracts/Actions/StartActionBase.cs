namespace WhatNow.Contracts.Actions
{
	public abstract class StartActionBase<TOutput> : ActionBase<NullObject, TOutput>
	{
		public abstract TOutput Execute();

		private protected override TOutput DoExecuteAction(NullObject input) => Execute();
	}
}