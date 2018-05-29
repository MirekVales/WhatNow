using System;

namespace WhatNow.Contracts.Actions
{
	public abstract class ActionBase<TInput, TOutput> : IAction
    { 
        protected readonly object accessLock = new object();

        bool breakRequested;
        BreakRequestReason breakRequestReason;
        bool finished;

        public bool BreakRequested
        {
            get { lock (accessLock) return breakRequested; }
            private set { lock (accessLock) breakRequested = value; }
        }
        public BreakRequestReason BreakRequestReason
        {
            get { lock (accessLock) return breakRequestReason; }
            private set { lock (accessLock) breakRequestReason = value; }
        }
        public bool Finished
        {
            get { lock (accessLock) return finished; }
            private set { lock (accessLock) finished = value; }
        }

        private protected abstract TOutput DoExecuteAction(TInput input);

        public object ExecuteAction(object input)
        {
            try
            {
                var output = DoExecuteAction((TInput)input);
				Finished = true;
				return output;
			}
			catch (Exception e)
            {
                RequestBreak(new BreakRequestException(GetType(), e));
                return NullObject.Value;
            }
        }

        protected void RequestBreak(BreakRequestReason reason)
        {
            BreakRequestReason = reason;
            BreakRequested = true;
        }
    }
}
