using System;

namespace WhatNow.Contracts
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
