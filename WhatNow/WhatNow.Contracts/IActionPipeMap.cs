using System;
using System.Collections.Generic;

namespace WhatNow.Contracts
{
    public interface IActionPipeMap
    {
        IEnumerable<Type> UsedActionTypes { get; }

        IEnumerable<Type> GetNext(IEnumerable<ActionBase> currents);

        int ComparePosition(Type t1, Type t2);

        int GetPosition(Type t);
    }
}