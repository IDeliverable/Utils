using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading;

namespace IDeliverable.Utils.Core.Collections
{
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
        private int mIsUpdatingCount = 0;
        private readonly List<string> mPropertiesChangedDuringUpdate = new List<string>();
        private bool mCollectionChangedDuringUpdate;

        public virtual void ReplaceItemAt(int index, T item)
        {
            SetItem(index, item);
        }

        public virtual void AddRange(IEnumerable<T> collection)
        {
            foreach (var item in collection)
                Add(item);
        }

        public virtual void InsertRange(int index, IEnumerable<T> collection)
        {
            var i = index;
            foreach (var item in collection)
            {
                Insert(i, item);
                i += 1;
            }
        }

        public virtual void RemoveRange(IEnumerable<T> collection)
        {
            foreach (var item in collection)
                Remove(item);
        }

        public bool IsUpdating => mIsUpdatingCount > 0;

        public void BeginUpdate()
        {
            if (mIsUpdatingCount == 0)
                BeginUpdateInternal();

            mIsUpdatingCount += 1;
        }

        public void EndUpdate()
        {
            if (mIsUpdatingCount == 0)
                throw new InvalidOperationException($"{nameof(EndUpdate)}() cannot be called without a matching call to {nameof(BeginUpdate)}() first.");

            mIsUpdatingCount -= 1;

            if (mIsUpdatingCount == 0)
                EndUpdateInternal();
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
    }
}
