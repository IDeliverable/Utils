namespace IDeliverable.Utils.Core
{
    public class Reference<T>
        where T : struct
    {
        public Reference(T value)
        {
            Value = value;
        }

        public T Value { get; }
    }
}
