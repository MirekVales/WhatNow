﻿using System;
using System.Collections.Generic;

namespace WhatNow.Contracts.Actions
{
    public interface IActionPipeMap : IEnumerable<Type[]>
    {
        int MaxDegreeOfParallelism { get; }

        IEnumerable<Type> UsedActionTypes { get; }

        IEnumerable<Type> GetEntryPoints();

        IEnumerable<Type> GetNext(IEnumerable<IAction> currents);

        IEnumerable<Type> GetNext(Type current);

        int ComparePosition(Type t1, Type t2);

        int GetPosition(Type t);
    }
}