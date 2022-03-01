namespace WhatNow.Essentials
{
    using System;
    using System.Diagnostics;

    public class BlockStopwatch : IDisposable
    {
        readonly Stopwatch stopwatch;
        readonly Action<TimeSpan> closeAction;

        public TimeSpan ElapsedTime => stopwatch.Elapsed;

        public BlockStopwatch()
            => stopwatch = Stopwatch.StartNew();

        public BlockStopwatch(Action<TimeSpan> closeAction) : this()
            => this.closeAction = closeAction;

        public void Invoke()
            => closeAction?.Invoke(ElapsedTime);

        public void Dispose()
        {
            stopwatch.Stop();
            Invoke();
        }
    }
}
