using System;
using WhatNow.Contracts.Resources;

namespace WhatNow.Contracts.Actions
{
	public interface IAction
	{
		bool BreakRequested { get; }

		BreakRequestReason BreakRequestReason { get; }

		bool Finished { get; }

		/// <summary>
		/// Executes the action without generic type input, output. Not intended to be called outside of the framework
		/// </summary>
		object ExecuteUntyped(IResourceManager resourceManager, object input);

		Type InputType { get; }

		Type OutputType { get; }
	}
}
