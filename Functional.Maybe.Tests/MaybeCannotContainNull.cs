using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Functional.Maybe.Tests
{
	[TestClass]
	public class MaybeCannotContainNull
	{
		private class User
		{
			public string Name { get; set; }
		}

		[TestMethod]
		public void WhenSelectingNull_GettingNothing()
		{
			var user = new User { Name = null };

			var maybeUser = user.ToMaybe();

			Assert.AreEqual(Maybe<string>.Nothing, maybeUser.Select(_ => _.Name));

		}
	}
}
