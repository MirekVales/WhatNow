using System;
using WhatNow.Contracts.Exceptions;
using WhatNow.Contracts.Resources;

namespace WhatNow.Contracts.Actions
{
    public abstract class ActionBase<TInput, TOutput> : IAction
    {
        protected readonly object accessLock = new object();

        IResourceManager resourceManager;

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

        /// <summary>
        /// Executes the action without generic type input, output. Not intended to be called outside of the framework
        /// </summary>
        public object ExecuteUntyped(IResourceManager resourceManager, object input)
        {
            using (resourceManager.CreateUseScope(instance => this.resourceManager = instance))
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
        }

        public Type InputType => typeof(TInput);

        public Type OutputType => typeof(TOutput);

        protected void RequestBreak(BreakRequestReason reason)
        {
            BreakRequestReason = reason;
            BreakRequested = true;
        }

        public T Access<T>(Enum resourceId)
            where T : IAccessableResource
            => resourceManager.Access<T>(resourceId).Resource;
    }
}
