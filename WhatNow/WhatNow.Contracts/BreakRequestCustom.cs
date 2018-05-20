using System;

namespace WhatNow.Contracts
{
    public class BreakRequestCustom : BreakRequestReason
    {
        public string Reason { get; }

        public object Tag { get; }

        public BreakRequestCustom(Type actionType, string reason, object tag = null)
            : base(actionType, reason)
        {
            Reason = reason;
            Tag = tag;
        }
    }
}