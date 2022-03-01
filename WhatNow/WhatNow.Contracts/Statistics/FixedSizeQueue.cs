namespace WhatNow.Contracts.Statistics
{
    using System.Collections.Generic;

    public class FixedSizedQueue<T> : Queue<T>
    {
        public int MaxCapacity { get; }

        public FixedSizedQueue(int capacity) : base(capacity)
            => MaxCapacity = capacity;

        public new void Enqueue(T item)
        {
            if (MaxCapacity != 0 && MaxCapacity < Count + 1)
                Dequeue();

            base.Enqueue(item);
        }
    }
}
