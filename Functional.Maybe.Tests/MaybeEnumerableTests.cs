using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace Functional.Maybe.Tests
{
    [TestClass]
    public class MaybeEnumerableTests
    {
        [TestMethod]
        public void WhereValueExist_Should_remove_Nothing_values()
        {
            var sequence = new Maybe<int>[] { 1.ToMaybe(), Maybe<int>.Nothing, 2.ToMaybe() };
            int[] expected = { 1, 2 };

            var actual = sequence.WhereValueExist().ToArray();

            Assert.AreEqual(expected.Length, actual.Length);
            for (int i = 0; i < expected.Length; i++)
            {
                Assert.AreEqual(expected[i], actual[i]);
            }
        }
    }
}
