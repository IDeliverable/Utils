using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading;

namespace IDeliverable.Utils.Core.Collections.BubbleChange
{
    public class BubbleChangeCollection<T> : BatchingCollection<T>, IBubbleChange
    {
        public BubbleChangeCollection(SynchronizationContext syncContext = null)
            : base(syncContext)
        {
        }

        public BubbleChangeCollection(IEnumerable<T> collection, SynchronizationContext syncContext = null)
            : base(collection, syncContext)
        {
            foreach (var i in collection.OfType<IBubbleChange>())
                i.BubbleChange += Item_BubbleChange;
        }

        private BubbleChangeEventArgs mBubbleChangeDuringUpdate;

        public event BubbleChangeEventHandler BubbleChange;

        protected override void BeginUpdateInternal()
        {
            base.BeginUpdateInternal();
            mBubbleChangeDuringUpdate = new BubbleChangeEventArgs();
        }

        protected override void EndUpdateInternal()
        {
            base.EndUpdateInternal();

            if (mBubbleChangeDuringUpdate != null)
            {
                OnBubbleChange(this, mBubbleChangeDuringUpdate);
                mBubbleChangeDuringUpdate = null;
            }
        }

        protected virtual void OnBubbleChange(object sender, BubbleChangeEventArgs e)
        {
            if (IsUpdating)
                mBubbleChangeDuringUpdate.MergeFrom(e);
            else if (e.Operations.Any())
                BubbleChange?.Invoke(sender, e);
        }

        protected override void ClearItems()
        {
            foreach (var i in Items.OfType<IBubbleChange>())
                i.BubbleChange -= Item_BubbleChange;

            base.ClearItems();
        }

        protected override void InsertItem(int index, T item)
        {
            if (item is IBubbleChange bubbleChange)
                bubbleChange.BubbleChange += Item_BubbleChange;

            base.InsertItem(index, item);
        }

        protected override void RemoveItem(int index)
        {
            var item = Items[index];

            if (item is IBubbleChange bubbleChange)
                bubbleChange.BubbleChange -= Item_BubbleChange;

            base.RemoveItem(index);
        }

        protected override void SetItem(int index, T item)
        {
            var prevItem = Items[index];

            if (prevItem is IBubbleChange prevBubbleChange)
                prevBubbleChange.BubbleChange -= Item_BubbleChange;

            if (item is IBubbleChange bubbleChange)
                bubbleChange.BubbleChange += Item_BubbleChange;

            base.SetItem(index, item);
        }

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            base.OnCollectionChanged(e);

            var changeType = default(BubbleChangeType);
            IEnumerable<object> items = null;

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    changeType = BubbleChangeType.ItemAdded;
                    var newItems = (object[])Array.CreateInstance(typeof(object), e.NewItems.Count);
                    e.NewItems.CopyTo(newItems, 0);
                    items = newItems;
                    break;

                case NotifyCollectionChangedAction.Remove:
                    changeType = BubbleChangeType.ItemRemoved;
                    var oldItems = (object[])Array.CreateInstance(typeof(object), e.OldItems.Count);
                    e.OldItems.CopyTo(oldItems, 0);
                    items = oldItems;
                    break;
            }

            if (items != null)
                OnBubbleChange(this, new BubbleChangeEventArgs(items.Select(x => new BubbleChangeOperation(changeType, x))));
            else
                OnBubbleChange(this, new BubbleChangeEventArgs());
        }

        private void Item_BubbleChange(object sender, BubbleChangeEventArgs e)
        {
            OnBubbleChange(this, e);
        }
    }
}
