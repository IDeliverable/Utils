using System;
using System.Threading;
using System.Threading.Tasks;

namespace IDeliverable.Utils.Core
{
    public class SimpleTimer
    {
        public SimpleTimer(Action callback)
        {
            mCallback = callback;
        }

        private readonly Action mCallback;
        private TimeSpan mInterval = Timeout.InfiniteTimeSpan;
        private CancellationTokenSource mTokenSource;

        public TimeSpan Interval
        {
            get => mInterval;
            set
            {
                if (mInterval != value)
                {
                    if (mTokenSource != null)
                    {
                        mTokenSource.Cancel();
                        mTokenSource.Dispose();
                        mTokenSource = null;
                    }

                    mInterval = value;

                    if (mInterval != Timeout.InfiniteTimeSpan)
                    {
                        mTokenSource = new CancellationTokenSource();
                        Worker(null);
                    }
                }
            }
        }

        private void Worker(Task delayTask)
        {
            try
            {
                mCallback();
            }
            catch
            {
                // No use doing anything if callback throws.
            }

            Task.Delay(mInterval, mTokenSource.Token).ContinueWith(Worker, mTokenSource.Token, TaskContinuationOptions.OnlyOnRanToCompletion, TaskScheduler.Default);
        }
    }
}