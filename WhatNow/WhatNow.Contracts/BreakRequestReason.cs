using System;

namespace WhatNow.Contracts
{
    public abstract class BreakRequestReason
    {
        public Type ActionType { get; }

        protected BreakRequestReason(Type actionType)
        {
            ActionType = actionType;        
        }
    }
}
