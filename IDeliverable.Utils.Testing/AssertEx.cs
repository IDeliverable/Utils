using Newtonsoft.Json;
using Xunit;

namespace IDeliverable.Utils.Testing
{
    public static class AssertEx
    {
        /// <summary>
        /// Compares the two values by value.
        /// </summary>
        public static void EqualByValue<T>(this T expectedValue, T actualValue)
        {
            var actualJson = JsonConvert.SerializeObject(actualValue, Formatting.None);
            var expectedJson = JsonConvert.SerializeObject(expectedValue, Formatting.None);
            Assert.Equal(expectedJson, actualJson);
        }
    }
}
