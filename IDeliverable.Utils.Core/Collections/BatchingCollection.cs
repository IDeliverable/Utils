using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading;

namespace IDeliverable.Utils.Core.Collections
{
    /// <summary>
    /// Represents an observable collection that also provides the ability to raise collection and
    /// property change events on a given <see cref="SynchronizationContext"/>, to coalesce (batch) any
    /// such events during a sequence of collection modifications, and to obtain a snapshot copy of
    /// the collection in a thread-safe manner.
    /// </summary>
    /// <typeparam name="T">The typ of the elements in the collection.</typeparam>
    public class BatchingCollection<T> : ObservableCollection<T>
    {
        public BatchingCollection(SynchronizationContext syncContext = null)
        {
            mSyncContext = syncContext;
        }

        public BatchingCollection(IEnumerable<T> collection, SynchronizationContext syncContext = null) : base(collection)
        {
            mSyncContext = syncContext;
        }

        private readonly SynchronizationContext mSyncContext;
        private readonly List<string> mPropertiesChangedDuringUpdate = new List<string>();
        private readonly ReaderWriterLockSlim mSnapshotLock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

        private int mIsUpdatingCount = 0;
        private bool mCollectionChangedDuringUpdate;

        public virtual void ReplaceItemAt(int index, T item)
        {
            SetItem(index, item);
        }

        public virtual void AddRange(IEnumerable<T> collection)
        {
            mSnapshotLock.EnterWriteLock();

            try
            {
                foreach (var item in collection)
                    Add(item);
            }
            finally
            {
                mSnapshotLock.ExitWriteLock();
            }
        }

        public virtual void InsertRange(int index, IEnumerable<T> collection)
        {
            mSnapshotLock.EnterWriteLock();

            try
            {
                var i = index;
                foreach (var item in collection)
                {
                    Insert(i, item);
                    i += 1;
                }
            }
            finally
            {
                mSnapshotLock.ExitWriteLock();
            }
        }

        public virtual void RemoveRange(IEnumerable<T> collection)
        {
            mSnapshotLock.EnterWriteLock();

            try
            {
                foreach (var item in collection)
                    Remove(item);
            }
            finally
            {
                mSnapshotLock.ExitWriteLock();
            }
        }

        public bool IsUpdating => mIsUpdatingCount > 0;

        public void BeginUpdate()
        {
            if (Interlocked.Increment(ref mIsUpdatingCount) == 1)
                BeginUpdateInternal();
        }

        public void EndUpdate()
        {
            if (mIsUpdatingCount == 0)
                throw new InvalidOperationException($"{nameof(EndUpdate)}() cannot be called without a matching call to {nameof(BeginUpdate)}() first.");

            if (Interlocked.Decrement(ref mIsUpdatingCount) == 0)
                EndUpdateInternal();
        }

        public virtual IEnumerable<T> Snapshot()
        {
            mSnapshotLock.EnterReadLock();

            try
            {
                return this.ToArray();
            }
            finally
            {
                mSnapshotLock.ExitReadLock();
            }
        }

        protected virtual void BeginUpdateInternal()
        {
            mPropertiesChangedDuringUpdate.Clear();
            mCollectionChangedDuringUpdate = false;
        }

        protected virtual void EndUpdateInternal()
        {
            var changedPropertiesQuery = mPropertiesChangedDuringUpdate.Select(x => new PropertyChangedEventArgs(x));

            foreach (var e in changedPropertiesQuery)
                OnPropertyChanged(e);

            mPropertiesChangedDuringUpdate.Clear();

            if (mCollectionChangedDuringUpdate)
            {
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                mCollectionChangedDuringUpdate = false;
            }
        }

        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (mIsUpdatingCount > 0)
                mPropertiesChangedDuringUpdate.Add(e.PropertyName);
            else
            {
                if (mSyncContext != null)
                    mSyncContext.Post((state) => base.OnPropertyChanged(e), null);
                else
                    base.OnPropertyChanged(e);
            }
        }

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (mIsUpdatingCount > 0)
                mCollectionChangedDuringUpdate = true;
            else
            {
                if (mSyncContext != null)
                    mSyncContext.Post((state) => base.OnCollectionChanged(e), null);
                else
                    base.OnCollectionChanged(e);
            }
        }

        protected override void ClearItems()
        {
            mSnapshotLock.EnterWriteLock();

            try
            {
                base.ClearItems();
            }
            finally
            {
                mSnapshotLock.ExitWriteLock();
            }
        }

        protected override void InsertItem(int index, T item)
        {
            mSnapshotLock.EnterWriteLock();

            try
            {
                base.InsertItem(index, item);
            }
            finally
            {
                mSnapshotLock.ExitWriteLock();
            }
        }

        protected override void RemoveItem(int index)
        {
            mSnapshotLock.EnterWriteLock();

            try
            {
                base.RemoveItem(index);
            }
            finally
            {
                mSnapshotLock.ExitWriteLock();
            }
        }

        protected override void SetItem(int index, T item)
        {
            mSnapshotLock.EnterWriteLock();

            try
            {
                base.SetItem(index, item);
            }
            finally
            {
                mSnapshotLock.ExitWriteLock();
            }
        }

        protected override void MoveItem(int oldIndex, int newIndex)
        {
            mSnapshotLock.EnterWriteLock();

            try
            {
                base.MoveItem(oldIndex, newIndex);
            }
            finally
            {
                mSnapshotLock.ExitWriteLock();
            }
        }
    }
}
