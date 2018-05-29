namespace WhatNow.Contracts.Actions
{
	public abstract class PipelineActionBase<TInput, TOutput> : ActionBase<TInput, TOutput>
	{
		protected PipelineActionBase(ActionToken actionToken) : base(actionToken)
		{
		}

		protected abstract TOutput Execute(TInput input);

		private protected override TOutput DoExecuteAction(TInput input) => Execute(input);
	}
}
