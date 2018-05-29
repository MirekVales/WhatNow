namespace WhatNow.Contracts.Actions
{
	public abstract class PipelineActionBase<TInput, TOutput> : ActionBase<TInput, TOutput>
	{
		public abstract TOutput Execute(TInput input);

		private protected override TOutput DoExecuteAction(TInput input) => Execute(input);
	}
}
