namespace IDeliverable.Utils.Core
{
    public static class NumberExtensions
    {
        public static int Clamp(this int value, int min, int max)
        {
            return value < min ? min : value > max ? max : value;
        }
    }
}
