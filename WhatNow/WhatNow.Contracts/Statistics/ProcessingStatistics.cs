using System;
using System.Linq;
using System.Collections.Generic;

namespace WhatNow.Contracts.Statistics
{
    public class ProcessingStatistics
    {
        public IEnumerable<ProcessingStatisticsItem> Items { get; }

        public TimeSpan TotalTime
            => TimeSpan.FromMilliseconds(
                Items.Sum(i => i.ExecutionCount * i.AverageDuration.TotalMilliseconds));

        public int TotalExecutions
            => Items.Max(i => i.ExecutionCount);

        public bool IsFullyProcessed
            => Items
            .OrderByDescending(i => i.LocationInActionMap)
            .FirstOrDefault()
            ?.ExecutionCount > 0;

        public bool HasConsistentExecutionCounts
            => Items.Count() < 2
            || Items.All(i => i.ExecutionCount == Items.First().ExecutionCount);

        public ProcessingStatistics(IEnumerable<ProcessingStatisticsItem> items)
        {
            Items = items.ToArray();
        }

        char Mark(bool b)
            => b ? 'x' : 'o';

        public string Header
            => $"{TotalTime} [{Mark(IsFullyProcessed)}|{Mark(HasConsistentExecutionCounts)}]";

        public override string ToString()
        {
            return Header
                + Environment.NewLine
                + string.Join(Environment.NewLine, Items.OrderBy(i => i.LocationInActionMap).Select(i => i.ToString()));
        }
    }
}
