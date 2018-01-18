using System;
using System.Collections.Specialized;

namespace IDeliverable.Utils.Core.Collections
{
    public class GroupItemsCollectionChangedEventArgs<TGroupKey> : EventArgs
    {
        public GroupItemsCollectionChangedEventArgs(TGroupKey groupKey, NotifyCollectionChangedEventArgs collectionChangedArgs)
        {
            GroupKey = groupKey;
            CollectionChangedArgs = collectionChangedArgs;
        }

        public TGroupKey GroupKey { get; }
        public NotifyCollectionChangedEventArgs CollectionChangedArgs { get; }
    }
}