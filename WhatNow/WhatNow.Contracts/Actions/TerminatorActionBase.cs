namespace WhatNow.Contracts.Actions
{
	public abstract class TerminatorActionBase<TInput> : ActionBase<TInput, NullObject>
	{
		protected TerminatorActionBase(ActionToken actionToken) : base(actionToken)
		{
		}

		protected abstract void Execute(TInput input);

		private protected override NullObject DoExecuteAction(TInput input)
		{
			Execute(input);
			return NullObject.Value;
		}
	}
}
