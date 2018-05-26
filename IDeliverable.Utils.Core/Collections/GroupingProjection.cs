using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using IDeliverable.Utils.Core.CollectionExtensions;

namespace IDeliverable.Utils.Core.Collections
{
    public partial class GroupingProjection<TGroupKey, TOrderKey, TItem> : BatchingCollection<GroupingProjection<TGroupKey, TOrderKey, TItem>.Group>, IDisposable
        where TGroupKey : IComparable<TGroupKey>
        where TOrderKey : IComparable<TOrderKey>
    {
        public GroupingProjection(IEnumerable<TItem> sourceCollection, Func<TItem, TGroupKey> groupKeySelector)
            : this(sourceCollection, groupKeySelector, item => default(TOrderKey))
        {
        }

        public GroupingProjection(IEnumerable<TItem> sourceCollection, Func<TItem, TGroupKey> groupKeySelector, Func<TItem, TOrderKey> orderKeySelector)
            : this(sourceCollection, groupKeySelector, orderKeySelector, Comparer<TOrderKey>.Default)
        {
        }

        public GroupingProjection(IEnumerable<TItem> sourceCollection, Func<TItem, TGroupKey> groupKeySelector, Func<TItem, TOrderKey> orderKeySelector, IComparer<TOrderKey> orderKeyComparer)
            : this(sourceCollection, groupKeySelector, Comparer<TGroupKey>.Default, orderKeySelector, orderKeyComparer, item => true)
        {
        }

        public GroupingProjection(IEnumerable<TItem> sourceCollection, Func<TItem, TGroupKey> groupKeySelector, IComparer<TGroupKey> groupKeyComparer, Func<TItem, TOrderKey> orderKeySelector)
            : this(sourceCollection, groupKeySelector, groupKeyComparer, orderKeySelector, Comparer<TOrderKey>.Default, item => true)
        {
        }

        public GroupingProjection(IEnumerable<TItem> sourceCollection, Func<TItem, TGroupKey> groupKeySelector, IComparer<TGroupKey> groupKeyComparer, Func<TItem, TOrderKey> orderKeySelector, IComparer<TOrderKey> orderKeyComparer)
            : this(sourceCollection, groupKeySelector, groupKeyComparer, orderKeySelector, orderKeyComparer, item => true)
        {
        }

        public GroupingProjection(IEnumerable<TItem> sourceCollection, Func<TItem, TGroupKey> groupKeySelector, Func<TItem, bool> filterPredicate)
            : this(sourceCollection, groupKeySelector, Comparer<TGroupKey>.Default, item => default(TOrderKey), Comparer<TOrderKey>.Default, filterPredicate)
        {
        }

        public GroupingProjection(IEnumerable<TItem> sourceCollection, Func<TItem, TGroupKey> groupKeySelector, IComparer<TGroupKey> groupKeyComparer, Func<TItem, TOrderKey> orderKeySelector, IComparer<TOrderKey> orderKeyComparer, Func<TItem, bool> filterPredicate)
        {
            mSourceCollection = sourceCollection;
            mGroupKeySelector = groupKeySelector;
            mGroupKeyComparer = groupKeyComparer;
            mOrderKeySelector = orderKeySelector;
            mOrderKeyComparer = orderKeyComparer;
            mFilterPredicate = filterPredicate;

            Project();

            CollectionChanged += GroupingProjection_CollectionChanged;
            if (mSourceCollection is INotifyCollectionChanged observableCollection)
                observableCollection.CollectionChanged += SourceCollection_CollectionChanged;

            AddEventHandlersToGroups(this);
            AddEventHandlersToItems(mSourceCollection);
        }

        public delegate void GroupItemsCollectionChangedEventHandler(object sender, GroupItemsCollectionChangedEventArgs<TGroupKey> e);

        public event GroupItemsCollectionChangedEventHandler GroupItemsCollectionChanged;

        private readonly IEnumerable<TItem> mSourceCollection;
        private readonly Func<TItem, TGroupKey> mGroupKeySelector;
        private readonly IComparer<TGroupKey> mGroupKeyComparer;
        private readonly Func<TItem, TOrderKey> mOrderKeySelector;
        private readonly IComparer<TOrderKey> mOrderKeyComparer;
        private readonly Func<TItem, bool> mFilterPredicate;
        private readonly object mProjectionLock = new object();

        private bool mIsDisposed;

        #region Modification blocking

        public new void Add(Group item)
        {
            throw new NotImplementedException("Manually modifying the groups in a GroupingCollectionView is not supported; modify the underlying source collection instead.");
        }

        public new void Insert(int index, Group item)
        {
            throw new NotImplementedException("Manually modifying the groups in a GroupingCollectionView is not supported; modify the underlying source collection instead.");
        }

        public new void Remove(Group item)
        {
            throw new NotImplementedException("Manually modifying the groups in a GroupingCollectionView is not supported; modify the underlying source collection instead.");
        }

        public new void RemoveAt(int index)
        {
            throw new NotImplementedException("Manually modifying the groups in a GroupingCollectionView is not supported; modify the underlying source collection instead.");
        }

        public new void Move(int oldIndex, int newIndex)
        {
            throw new NotImplementedException("Manually modifying the groups in a GroupingCollectionView is not supported; modify the underlying source collection instead.");
        }

        public new void Clear()
        {
            throw new NotImplementedException("Manually modifying the groups in a GroupingCollectionView is not supported; modify the underlying source collection instead.");
        }

        #endregion

        public void Dispose()
        {
            if (!mIsDisposed)
            {
                CollectionChanged -= GroupingProjection_CollectionChanged;
                if (mSourceCollection is INotifyCollectionChanged observableCollection)
                    observableCollection.CollectionChanged -= SourceCollection_CollectionChanged;

                RemoveEventHandlersFromGroups(this);
                RemoveEventHandlersFromItems(mSourceCollection);

                mIsDisposed = true;
            }
        }

        private void Project()
        {
            lock (mProjectionLock)
            {
                var projection =
                    mSourceCollection.ToArray()
                        .Where(mFilterPredicate)
                        .OrderBy(mOrderKeySelector, mOrderKeyComparer)
                        .GroupBy(mGroupKeySelector)
                        .Select(group => new Group(group.Key, group))
                        .OrderBy(x => x.Key, mGroupKeyComparer)
                        .ToArray();

                projection.SynchronizeToView(this, CollectionSynchronizationMode.KeepOrderByMove);

                foreach (var group in projection)
                {
                    var targetGroup = Items.Single(x => Equals(x.GroupKey, group.GroupKey));
                    group.Items.SynchronizeToView(targetGroup.ItemsInternal, CollectionSynchronizationMode.KeepOrderByMove);
                }
            }
        }

        private void SourceObject_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            Project();
        }

        private void GroupingProjection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            var newGroups = e.NewItems?.Cast<Group>();
            var oldGroups = e.OldItems?.Cast<Group>();

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    AddEventHandlersToGroups(newGroups);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    RemoveEventHandlersFromGroups(oldGroups);
                    break;
                case NotifyCollectionChangedAction.Replace:
                case NotifyCollectionChangedAction.Reset:
                    RemoveEventHandlersFromGroups(oldGroups);
                    AddEventHandlersToGroups(newGroups);
                    break;
            }
        }

        private void GroupItems_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            var groupKeyQuery =
                from g in Items.ToArray() // ToArray() to avoid concurrency issues.
                where g.Items == sender
                select g.GroupKey;

            GroupItemsCollectionChanged?.Invoke(this, new GroupItemsCollectionChangedEventArgs<TGroupKey>(groupKeyQuery.FirstOrDefault(), e));
        }

        private void SourceCollection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    AddEventHandlersToItems(e.NewItems.Cast<TItem>());
                    break;

                case NotifyCollectionChangedAction.Remove:
                    RemoveEventHandlersFromItems(e.OldItems.Cast<TItem>());
                    break;

                case NotifyCollectionChangedAction.Replace:
                    AddEventHandlersToItems(e.NewItems.Cast<TItem>());
                    RemoveEventHandlersFromItems(e.OldItems.Cast<TItem>());
                    break;

                case NotifyCollectionChangedAction.Reset:
                    var allItemsQuery =
                        from @group in Items
                        from i in @group.Items
                        select i;
                    RemoveEventHandlersFromItems(allItemsQuery);
                    AddEventHandlersToItems(mSourceCollection);
                    break;
            }

            Project();
        }

        private void AddEventHandlersToItems(IEnumerable<TItem> items)
        {
            foreach (var i in items.ToArray()) // ToArray() to avoid concurrency issues.
            {
                if (i is INotifyPropertyChanged observableItem)
                    observableItem.PropertyChanged += SourceObject_PropertyChanged;
            }
        }

        private void RemoveEventHandlersFromItems(IEnumerable<TItem> items)
        {
            foreach (var i in items.ToArray()) // ToArray() to avoid concurrency issues.
            {
                if (i is INotifyPropertyChanged observableItem)
                    observableItem.PropertyChanged -= SourceObject_PropertyChanged;
            }
        }

        private void AddEventHandlersToGroups(IEnumerable<Group> groups)
        {
            if (groups == null)
                return;

            foreach (var g in groups)
                ((INotifyCollectionChanged)g.Items).CollectionChanged += GroupItems_CollectionChanged;
        }

        private void RemoveEventHandlersFromGroups(IEnumerable<Group> groups)
        {
            if (groups == null)
                return;

            foreach (var g in groups)
                ((INotifyCollectionChanged)g.Items).CollectionChanged -= GroupItems_CollectionChanged;
        }

        public sealed class Group : IEquatable<Group>, IGrouping<TGroupKey, TItem>
        {
            public Group(TGroupKey groupKey) : this(groupKey, Enumerable.Empty<TItem>())
            {
            }

            public Group(TGroupKey groupKey, IEnumerable<TItem> items)
            {
                GroupKey = groupKey;
                ItemsInternal = new ObservableCollection<TItem>(items);
                Items = new ReadOnlyObservableCollection<TItem>(ItemsInternal);
            }

            public TGroupKey GroupKey { get; }
            public TGroupKey Key => GroupKey;

            public ReadOnlyObservableCollection<TItem> Items { get; }
            internal ObservableCollection<TItem> ItemsInternal { get; }

            public bool Equals(Group other)
            {
                return Equals(GroupKey, other.GroupKey);
            }

            public override bool Equals(object obj)
            {
                if (obj is Group otherGroup)
                    return Equals(otherGroup);

                return false;
            }

            public IEnumerator<TItem> GetEnumerator()
            {
                return Items.GetEnumerator();
            }

            public override int GetHashCode()
            {
                return GroupKey?.GetHashCode() ?? 0;
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }
    }
}