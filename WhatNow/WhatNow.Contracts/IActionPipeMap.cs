using System;
using System.Collections.Generic;
using WhatNow.Contracts.Actions;

namespace WhatNow.Contracts
{
    public interface IActionPipeMap
    {
        IEnumerable<Type> UsedActionTypes { get; }

        IEnumerable<Type> GetNext(IEnumerable<IAction> currents);

        int ComparePosition(Type t1, Type t2);

        int GetPosition(Type t);
    }
}