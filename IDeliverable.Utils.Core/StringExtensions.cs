using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace IDeliverable.Utils.Core
{
    public static class StringExtensions
    {
        public static string ReplaceAll(this string original, IDictionary<string, string> replacements)
        {
            var pattern = String.Format("({0})", String.Join("|", replacements.Keys.Select(Regex.Escape).ToArray()));

            return Regex.Replace(original, pattern, match => replacements[match.Value]);
        }
    }
}