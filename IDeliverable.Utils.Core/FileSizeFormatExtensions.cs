using System;

namespace IDeliverable.Utils.Core
{
    public static class FileSizeFormatExtensions
    {
        public static string ToFileSizeString(this decimal value)
        {
            return String.Format(new FileSizeFormatProvider(), "{0:fs}", value);
        }

        public static string ToFileSizeString(this long value)
        {
            return String.Format(new FileSizeFormatProvider(), "{0:fs}", value);
        }
    }
}
