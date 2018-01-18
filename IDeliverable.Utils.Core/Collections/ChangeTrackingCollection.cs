using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Xml.Serialization;
using IDeliverable.Utils.Core.Collections.BubbleChange;

namespace IDeliverable.Utils.Core.Collections
{
    public class ChangeTrackingCollection<T> : BubbleChangeCollection<T>, IChangeTracking, IXmlSerializable
    {
        public ChangeTrackingCollection(SynchronizationContext syncContext = null)
            : base(syncContext)
        {
            mAddedItems.CollectionChanged += AddedRemovedCollectionChanged;
            mRemovedItems.CollectionChanged += AddedRemovedCollectionChanged;
        }

        public ChangeTrackingCollection(IEnumerable<T> collection, SynchronizationContext syncContext = null)
            : base(collection, syncContext)
        {
            mAddedItems.CollectionChanged += AddedRemovedCollectionChanged;
            mRemovedItems.CollectionChanged += AddedRemovedCollectionChanged;

            foreach (var i in collection.OfType<INotifyPropertyChanged>())
                i.PropertyChanged += Item_PropertyChanged;

            foreach (var i in collection.OfType<IUniqueId>())
            {
                if (!mUniqueIdSet.Add(i.UniqueId))
                    throw new InvalidOperationException($"Item with unique ID '{i.UniqueId}' already exists in the collection.");
            }
        }

        private readonly ObservableCollection<T> mAddedItems = new ObservableCollection<T>();
        private readonly ObservableCollection<T> mRemovedItems = new ObservableCollection<T>();
        private readonly HashSet<Guid> mUniqueIdSet = new HashSet<Guid>();
        private bool mHasChanges;
        private bool mItemsHaveChanges;

        public virtual bool HasAddedItems => mAddedItems.Count > 0;
        public virtual bool HasRemovedItems => mRemovedItems.Count > 0;

        public virtual bool IsAdded(T item)
        {
            if (!Items.Contains(item))
                throw new ArgumentOutOfRangeException(nameof(item), "The specified item does not exist in this collection.");

            return mAddedItems.Contains(item);
        }

        public virtual bool IsRemoved(T item)
        {
            if (!Items.Contains(item) && !mRemovedItems.Contains(item))
                throw new ArgumentOutOfRangeException(nameof(item), "The specified item does not exist in this collection.");

            return mRemovedItems.Contains(item);
        }

        public virtual IEnumerable<T> AllItems => Items.Concat(mRemovedItems).ToArray();
        public virtual IEnumerable<T> AddedItems => mAddedItems.ToArray();
        public virtual IEnumerable<T> RemovedItems => mRemovedItems.ToArray();

        public virtual bool ItemsHaveChanges
        {
            get => mItemsHaveChanges;
            protected set
            {
                if (value != mItemsHaveChanges)
                {
                    var currentIsChanged = IsChanged;
                    var newIsChanged = mHasChanges || value;

                    mItemsHaveChanges = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(ItemsHaveChanges)));

                    if (newIsChanged != currentIsChanged)
                        OnPropertyChanged(new PropertyChangedEventArgs(nameof(IsChanged)));
                }
            }
        }

        public bool IsChanged
        {
            get => mHasChanges || mItemsHaveChanges;
            protected set
            {
                if (mHasChanges != value)
                {
                    mHasChanges = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(IsChanged)));
                }
            }
        }

        public virtual void AcceptChanges()
        {
            BeginUpdate();

            try
            {
                // NOTE: Removed items should not be considered as belonging to this 
                // collection and hence accept operations should not be cascaded 
                // to them.

                mAddedItems.Clear();
                mRemovedItems.Clear();

                foreach (var i in Items.OfType<IChangeTracking>())
                    i.AcceptChanges();
            }
            finally
            {
                EndUpdate();
            }
        }

        public virtual void AcceptChange(T item)
        {
            AcceptChange(item, applyToItem: true);
        }

        public virtual void AcceptChange(T item, bool applyToItem)
        {
            if (mAddedItems.Contains(item))
                mAddedItems.Remove(item);
            else if (mRemovedItems.Contains(item))
                mRemovedItems.Remove(item);
            else
                throw new ArgumentOutOfRangeException(nameof(item), "The specified item does not exist in this collection.");

            if (applyToItem && item is IChangeTracking changeTrackingItem)
                changeTrackingItem.AcceptChanges();
        }

        protected override void ClearItems()
        {
            foreach (var i in AllItems.OfType<INotifyPropertyChanged>())
                i.PropertyChanged -= Item_PropertyChanged;
            foreach (var i in Items.OfType<IUniqueId>())
                mUniqueIdSet.Remove(i.UniqueId);

            foreach (var i in Items)
            {
                if (!mAddedItems.Contains(i))
                    mRemovedItems.Add(i);
            }

            mAddedItems.Clear();

            base.ClearItems();

            CalcItemsHaveChanges();
        }

        protected override void InsertItem(int index, T item)
        {
            if (item is INotifyPropertyChanged observableItem)
                observableItem.PropertyChanged += Item_PropertyChanged;
            if (item is IUniqueId uniqueItem && !mUniqueIdSet.Add(uniqueItem.UniqueId))
                throw new InvalidOperationException($"Item with unique ID '{uniqueItem.UniqueId}' already exists in the collection.");

            if (mRemovedItems.Contains(item))
                mRemovedItems.Remove(item);
            else
                mAddedItems.Add(item);

            base.InsertItem(index, item);

            CalcItemsHaveChanges();
        }

        protected override void RemoveItem(int index)
        {
            var item = Items[index];

            if (item is INotifyPropertyChanged observableItem)
                observableItem.PropertyChanged -= Item_PropertyChanged;
            if (item is IUniqueId uniqueItem)
                mUniqueIdSet.Remove(uniqueItem.UniqueId);

            if (mAddedItems.Contains(Items[index]))
                mAddedItems.Remove(Items[index]);
            else
                mRemovedItems.Add(Items[index]);

            base.RemoveItem(index);

            CalcItemsHaveChanges();
        }

        protected override void SetItem(int index, T item)
        {
            var prevItem = Items[index];

            if (prevItem is INotifyPropertyChanged prevObservableItem)
                prevObservableItem.PropertyChanged -= Item_PropertyChanged;
            if (prevItem is IUniqueId prevUniqueItem)
                mUniqueIdSet.Remove(prevUniqueItem.UniqueId);

            if (mAddedItems.Contains(prevItem))
                mAddedItems.Remove(prevItem);
            else
                mRemovedItems.Add(prevItem);

            if (item is INotifyPropertyChanged observableItem)
                observableItem.PropertyChanged += Item_PropertyChanged;
            if (item is IUniqueId uniqueItem && !mUniqueIdSet.Add(uniqueItem.UniqueId))
                throw new InvalidOperationException($"Item with unique ID '{uniqueItem.UniqueId}' already exists in the collection.");

            if (mRemovedItems.Contains(item))
                mRemovedItems.Remove(item);
            else
                mAddedItems.Add(item);

            base.SetItem(index, item);

            CalcItemsHaveChanges();
        }

        private void AddedRemovedCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            IsChanged = mAddedItems.Count > 0 || mRemovedItems.Count > 0;
        }

        private void Item_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (sender is T && e.PropertyName == nameof(IChangeTracking.IsChanged))
            {
                var item = (T)sender;
                if (Items.Contains(item))
                {
                    if (item is IChangeTracking changeTrackingItem && changeTrackingItem.IsChanged)
                        ItemsHaveChanges = true;
                    else
                        CalcItemsHaveChanges();
                }
            }
        }

        private void CalcItemsHaveChanges()
        {
            ItemsHaveChanges = Items.OfType<IChangeTracking>().Any(x => x.IsChanged);
        }

        // INFO: The IXmlSerializable interface is implemented to force 
        // the WCF DataContractSerializer to serialize our other private 
        // fields, not just the items in our collection. More information 
        // here:
        // http://www.netknowledgenow.com/blogs/onmaterialize/archive/2007/01/18/Control-WCF-Serialization-of-Collections-with-IXmlSerializable.aspx

        #region "IXmlSerializable Members"

        private readonly DataContractSerializer mSerializer = new DataContractSerializer(typeof(T));

        XmlSchema IXmlSerializable.GetSchema()
        {
            return null;
        }

        void IXmlSerializable.ReadXml(XmlReader reader)
        {
            reader.Read();

            var xml = (XElement)XNode.ReadFrom(reader);

            foreach (var e in xml.Elements())
                Items.Add(DeserializeItem(e));

            foreach (var e in xml.Elements())
            {
                var item = DeserializeItem(e);
                mAddedItems.Add(item);
                Items.Add(item);
            }

            foreach (var e in xml.Elements())
                mRemovedItems.Add(DeserializeItem(e));

            // INFO: Events don't get properly rewired on deserialization 
            // with DataContractSerializer. The following code is the workaround. 
            // More information here: http://www.codeproject.com/KB/cs/FixingBindingListDeserial.aspx
            var tempList = new List<T>(Items);
            var index = 0;
            foreach (var tempItem in tempList)
            {
                base.SetItem(index, tempItem);
                index += 1;
            }
        }

        void IXmlSerializable.WriteXml(XmlWriter writer)
        {
            var xml = new XElement("UndoableCollection",
                new XElement("Items", from i in this where !mAddedItems.Contains(i) select SerializeItem(i)),
                new XElement("AddedItems", from i in mAddedItems select SerializeItem(i)),
                new XElement("RemovedItems", from i in mRemovedItems select SerializeItem(i)));
            xml.WriteTo(writer);
        }

        private XNode SerializeItem(T item)
        {
            using (var stream = new MemoryStream())
            {
                mSerializer.WriteObject(stream, item);
                stream.Seek(0, SeekOrigin.Begin);
                using (var reader = XmlReader.Create(stream))
                {
                    reader.Read();
                    return XNode.ReadFrom(reader);
                }
            }
        }

        private T DeserializeItem(XNode xml)
        {
            using (var stream = new MemoryStream())
            {
                using (var writer = XmlWriter.Create(stream))
                    xml.WriteTo(writer);

                stream.Seek(0, SeekOrigin.Begin);
                return (T)mSerializer.ReadObject(stream);
            }
        }

        #endregion
    }
}
