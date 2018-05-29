namespace WhatNow.Contracts.Actions
{
	public abstract class StartActionBase<TOutput> : ActionBase<NullObject, TOutput>
	{
		protected StartActionBase(ActionToken actionToken) : base(actionToken)
		{
		}

		protected abstract TOutput Execute();

		private protected override TOutput DoExecuteAction(NullObject input) => Execute();
	}
}