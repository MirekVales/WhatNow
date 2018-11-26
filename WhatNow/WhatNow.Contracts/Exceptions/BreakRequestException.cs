using System;
using WhatNow.Contracts.Actions;

namespace WhatNow.Contracts.Exceptions
{
    public class BreakRequestException : BreakRequestReason
    {
        public BreakRequestException(Type actionType, Exception exception)
            : base(actionType, exception.Message)
        {
            Exception = exception;
        }

        public Exception Exception { get; }
    }
}
