using System;

namespace IDeliverable.Utils.Core
{
    public class FileSizeFormatProvider : IFormatProvider, ICustomFormatter
    {
        private const decimal OneKiloByte = 1024m;
        private const decimal OneMegaByte = 1024m * OneKiloByte;
        private const decimal OneGigaByte = 1024m * OneMegaByte;
        private const decimal OneTeraByte = 1024m * OneGigaByte;
        private const string FormatString = "fs";

        object IFormatProvider.GetFormat(Type formatType)
        {
            return ReferenceEquals(formatType, typeof(ICustomFormatter)) ? this : null;
        }

        string ICustomFormatter.Format(string aFormat, object arg, IFormatProvider formatProvider)
        {
            if (aFormat == null || !aFormat.StartsWith(FormatString) || arg is string)
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
            if (value >= OneTeraByte)
            {
                size = value / OneTeraByte;
                suffix = "TB";
            }
            else if (value >= OneGigaByte)
            {
                size = value / OneGigaByte;
                suffix = "GB";
            }
            else if (value >= OneMegaByte)
            {
                size = value / OneMegaByte;
                suffix = "MB";
            }
            else if (value >= OneKiloByte)
            {
                size = value / OneKiloByte;
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
            var formattable = arg as IFormattable;
            return formattable != null ? formattable.ToString(format, formatProvider) : arg.ToString();
        }
    }
}