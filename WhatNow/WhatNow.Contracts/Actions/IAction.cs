namespace WhatNow.Contracts.Actions
{
	public interface IAction
	{
		bool BreakRequested { get; }

		BreakRequestReason BreakRequestReason { get; }

		bool Finished { get; }

		object ExecuteAction(object input);
	}
}
