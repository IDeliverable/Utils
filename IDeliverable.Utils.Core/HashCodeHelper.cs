using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace IDeliverable.Utils.Core
{
    public static class HashCodeHelper
    {
        public static int CombineHashCodes(params object[] args)
        {
            return CombineHashCodes(EqualityComparer<object>.Default, args);
        }

        public static int CombineHashCodes(IEqualityComparer comparer, params object[] args)
        {
            if (args == null) throw new ArgumentNullException(nameof(args));
            if (args.Length == 0) throw new ArgumentException("args");

            var hashcode = 0;

            unchecked
            {
                hashcode = args.Aggregate(hashcode, (current, arg) => (current << 5) - current ^ comparer.GetHashCode(arg));
            }

            return hashcode;
        }
    }
}