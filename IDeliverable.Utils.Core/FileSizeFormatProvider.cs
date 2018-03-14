using System;

namespace IDeliverable.Utils.Core
{
    public class FileSizeFormatProvider : IFormatProvider, ICustomFormatter
    {
        private const decimal mOneKiloByte = 1024m;
        private const decimal mOneMegaByte = 1024m * mOneKiloByte;
        private const decimal mOneGigaByte = 1024m * mOneMegaByte;
        private const decimal mOneTeraByte = 1024m * mOneGigaByte;
        private const string mFormatString = "fs";

        object IFormatProvider.GetFormat(Type formatType)
        {
            return ReferenceEquals(formatType, typeof(ICustomFormatter)) ? this : null;
        }

        string ICustomFormatter.Format(string aFormat, object arg, IFormatProvider formatProvider)
        {
            if (aFormat == null || !aFormat.StartsWith(mFormatString) || arg is string)
            {
                return FallbackFormat(aFormat, arg, formatProvider);
            }

            decimal value;

            try
            {
                value = Convert.ToDecimal(arg);
            }
            catch (Exception)
            {
                return FallbackFormat(aFormat, arg, formatProvider);
            }

            decimal size;
            string suffix;
            if (value >= mOneTeraByte)
            {
                size = value / mOneTeraByte;
                suffix = "TB";
            }
            else if (value >= mOneGigaByte)
            {
                size = value / mOneGigaByte;
                suffix = "GB";
            }
            else if (value >= mOneMegaByte)
            {
                size = value / mOneMegaByte;
                suffix = "MB";
            }
            else if (value >= mOneKiloByte)
            {
                size = value / mOneKiloByte;
                suffix = "KB";
            }
            else
            {
                size = value;
                suffix = "bytes";
            }

            var numberString = size.ToString("0.##");
            return $"{numberString} {suffix}";
        }

        private string FallbackFormat(string format, object arg, IFormatProvider formatProvider)
        {
            return arg is IFormattable formattable ? formattable.ToString(format, formatProvider) : arg.ToString();
        }
    }
}