using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;

namespace IDeliverable.Utils.Core.Collections
{
    public class RevertibleChangeTrackingCollection<T> : ChangeTrackingCollection<T>, IRevertibleChangeTracking
    {
        public RevertibleChangeTrackingCollection(SynchronizationContext syncContext = null)
            : base(syncContext)
        {
        }

        public RevertibleChangeTrackingCollection(IEnumerable<T> collection, SynchronizationContext syncContext = null)
            : base(collection, syncContext)
        {
        }

        public virtual void RejectChanges()
        {
            BeginUpdate();

            try
            {
                foreach (var i in AddedItems.OfType<IRevertibleChangeTracking>())
                    i.RejectChanges();

                foreach (var i in AddedItems)
                    RemoveItem(Items.IndexOf(i));

                foreach (var i in RemovedItems)
                    InsertItem(Items.Count, i);

                foreach (var i in Items.OfType<IRevertibleChangeTracking>())
                    i.RejectChanges();

                AcceptChanges(); // To clear the added and removed collections.
            }
            finally
            {
                EndUpdate();
            }
        }

        public virtual void RejectChange(T item)
        {
            RejectChange(item, applyToItem: true);
        }

        public virtual void RejectChange(T item, bool applyToItem)
        {
            if (AddedItems.Contains(item))
                RemoveItem(Items.IndexOf(item));
            else if (RemovedItems.Contains(item))
                InsertItem(Items.Count, item);
            else
                throw new ArgumentOutOfRangeException(nameof(item), "The specified item does not exist in this collection.");

            if (applyToItem && item is IRevertibleChangeTracking)
                ((IRevertibleChangeTracking)item).RejectChanges();
        }
    }
}