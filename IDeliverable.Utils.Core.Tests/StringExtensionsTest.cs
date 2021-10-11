using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IDeliverable.Utils.Core.Tests
{
    [TestClass]
    public class StringExtensionsTest
    {
        [DataTestMethod]
        [DataRow("1", "one")]
        [DataRow("2", "two")]
        [DataRow("12", "onetwo")]
        [DataRow("1 2", "one two")]
        [DataRow("1 2 (12)", "one two (onetwo)")]
        [DataRow("command:name", "command:name")]
        public void IsCorrect(string input, string expectedOutput)
        {
            var replacements = new Dictionary<string, string>()
            {
                { "1", "one" },
                { "2", "two" },
            };

            Assert.AreEqual(expectedOutput, input.ReplaceAll(replacements));
        }

        [DataTestMethod]
        [DataRow("command:name", "command:name")]
        [DataRow("command|name", "command-name")]
        public void IsCorrect_InvalidPathChars(string input, string expectedOutput)
        {
            var replacements =
                Path.GetInvalidPathChars()
                    .ToDictionary(x => x.ToString(), x => "-");

            Assert.AreEqual(expectedOutput, input.ReplaceAll(replacements));
        }
    }
}
