namespace IDeliverable.Utils.Core.Collections.BubbleChange
{
    public class BubbleChangeProperty
    {
        public BubbleChangeProperty(string name, object newValue)
        {
            Name = name;
            NewValue = newValue;
        }

        public string Name { get; }
        public object NewValue { get; }

        public bool Equals(BubbleChangeProperty other)
        {
            if (other is null)
                return false;
            if (ReferenceEquals(this, other))
                return true;
            return Equals(Name, other.Name);
        }

        public override bool Equals(object obj)
        {
            if (obj is BubbleChangeProperty)
                return Equals((BubbleChangeProperty)obj);
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = 47;
                if (Name != null)
                    hashCode = (hashCode * 53) ^ Name.GetHashCode();
                return hashCode;
            }
        }
    }
}
