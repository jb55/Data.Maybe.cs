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

		[TestMethod]
		public void Given_ThreeSome_UnionReturnsCollectionOfAll()
		{
			var one = 1.ToMaybe();
			var two = 2.ToMaybe();
			var three = 3.ToMaybe();

			var res = one.Union(two, three);
			Assert.AreEqual(3, res.Count());
			Assert.IsTrue(res.SequenceEqual(new[] { 1, 2, 3 }));
		}

		[TestMethod]
		public void Given_OneSome_UnionReturnsCollectionOfOne()
		{
			var one = 1.ToMaybe();
			var two = Maybe<int>.Nothing;

			var res = one.Union(two);
			Assert.AreEqual(1, res.Count());
			Assert.IsTrue(res.SequenceEqual(new[] { 1 }));
		}

		[TestMethod]
		public void Given_CollectionAndSome_UnionReturnsCollectionPlusSome()
		{
			var one = new[] { 1, 3 };
			var two = 2.ToMaybe();

			var res = one.Union(two);
			Assert.AreEqual(3, res.Count());
			Assert.IsTrue(res.SequenceEqual(new[] { 1, 3, 2 }));
		}
	}
}
