using System;
using System.Collections.Generic;

namespace IDeliverable.Utils.Core.Collections.BubbleChange
{
    public class BubbleChangeOperation
    {
        public BubbleChangeOperation(BubbleChangeType type, object item)
        {
            Type = type;
            Item = item;
            mProperties = new List<BubbleChangeProperty>();
        }

        public BubbleChangeOperation(BubbleChangeType type, object item, IEnumerable<BubbleChangeProperty> properties)
        {
            Type = type;
            Item = item;
            mProperties = new List<BubbleChangeProperty>(properties);
        }

        private List<BubbleChangeProperty> mProperties;

        public BubbleChangeType Type { get; }
        public object Item { get; }
        public IEnumerable<BubbleChangeProperty> Properties => mProperties;

        public void MergeFrom(BubbleChangeOperation other)
        {
            if (!Equals(other))
                throw new ArgumentException("Cannot merge when other object is not for the same item and type.");

            if (other.Type == BubbleChangeType.ItemChanged)
            {
                foreach (var otherProperty in other.Properties)
                {
                    if (!mProperties.Contains(otherProperty))
                        mProperties.Add(otherProperty);
                }
            }
        }

        public bool Equals(BubbleChangeOperation other)
        {
            if (other is null)
                return false;
            if (ReferenceEquals(this, other))
                return true;
            return Type.Equals(other.Type) && Equals(Item, other.Item);
        }

        public override bool Equals(object obj)
        {
            if (obj is BubbleChangeOperation)
                return Equals((BubbleChangeOperation)obj);
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = 47;
                hashCode = (hashCode * 53) ^ (int)Type;
                if (Item != null)
                    hashCode = (hashCode * 53) ^ Item.GetHashCode();
                return hashCode;
            }
        }
    }
}
