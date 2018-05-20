using System;

namespace WhatNow.Contracts
{
    public abstract class BreakRequestReason
    {
        public Type ActionType { get; }

        public string Message { get; protected set; }

        protected BreakRequestReason(Type actionType)
        {
            ActionType = actionType;        
        }

        protected BreakRequestReason(Type actionType, string message)
        {
            ActionType = actionType;
            Message = message;
        }
    }
}
