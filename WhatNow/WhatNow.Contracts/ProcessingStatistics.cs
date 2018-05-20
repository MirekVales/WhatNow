using System;

namespace WhatNow.Contracts
{
    public class ProcessingStatistics
    {
        public Type ActionType { get; }
        public int LocationInActionMap { get; }
        public int ExecutionCount { get; }
        public TimeSpan AverageDuration { get; }

        public ProcessingStatistics(Type actionType, int location, int executionCount, TimeSpan duration)
        {
            ActionType = actionType;
            LocationInActionMap = location;
            ExecutionCount = executionCount;
            AverageDuration = duration;
        }

        public override string ToString()
            => $"[{LocationInActionMap}] {ActionType.Name} ({ExecutionCount} @ {AverageDuration})";
    }
}
