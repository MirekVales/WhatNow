using System;

namespace WhatNow.Contracts
{
    public abstract class ActionBase
    {
        protected readonly object accessLock = new object();

        ActionToken token;
        bool breakRequested;
        BreakRequestReason breakRequestReason;
        bool finished;

        public ActionToken Token
        {
            get { lock (accessLock) return token; }
            private set { lock (accessLock) token = value; }
        }
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

        protected abstract void Execute();

        public void ExecuteAction()
        {
            try
            {
                Execute();
            }
            catch (Exception e)
            {
                RequestBreak(new BreakRequestException(GetType(), e));
                return;
            }
            Finished = true;
        }

        protected void RequestBreak(BreakRequestReason reason)
        {
            BreakRequestReason = reason;
            BreakRequested = true;
        }

        protected ActionBase(DependencyContainer dependencyContainer, ActionToken actionToken)
        {
            Token = actionToken;
        }
    }
}
