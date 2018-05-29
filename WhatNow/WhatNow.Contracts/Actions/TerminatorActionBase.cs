namespace WhatNow.Contracts.Actions
{
	public abstract class TerminatorActionBase<TInput> : ActionBase<TInput, NullObject>
	{
		public abstract void Execute(TInput input);

		private protected override NullObject DoExecuteAction(TInput input)
		{
			Execute(input);
			return NullObject.Value;
		}
	}
}
