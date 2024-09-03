// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the MIT License.  See LICENSE in the project root for license information.
using System.Text;
using NUnit.Framework;

namespace SpanKeyedCollections.Test
{
	[TestFixture]
	class BitwiseEqualityComparerTest
	{
		[TestCase(0, "", ExpectedResult = 0u)]
		[TestCase(42, "", ExpectedResult = 142593372u)]
		[TestCase(0, "MultipleOf04", ExpectedResult = 1581625577u)]
		[TestCase(42, "MultipleOf04", ExpectedResult = 2241771124u)]
		[TestCase(0, "1Over", ExpectedResult = 1850922185u)]
		[TestCase(42, "1Over", ExpectedResult = 2281453132u)]
		[TestCase(0, "ShortText", ExpectedResult = 763083174u)]
		[TestCase(42, "ShortText", ExpectedResult = 1710374205u)]
		public uint GetHashCode(int seed, string text)
		{
			var comparer = new BitwiseEqualityComparer<byte>(seed);
			return unchecked((uint)comparer.GetHashCode(Encoding.ASCII.GetBytes(text)));
		}

		[TestCase("foo", "foo", ExpectedResult = true)]
		[TestCase("foo", "bar", ExpectedResult = false)]
		[TestCase("foobar1", "foobar1", ExpectedResult = true)]
		[TestCase("foobar1", "foobar2", ExpectedResult = false)]
		[TestCase("foobarbazquux", "foobarbazquux", ExpectedResult = true)]
		[TestCase("foobarbazquux", "foobarbazQuux", ExpectedResult = false)]
		public bool Equals(string x, string y)
		{
			var comparer = new BitwiseEqualityComparer<byte>();
			return comparer.Equals(
				Encoding.UTF8.GetBytes(x),
				Encoding.UTF8.GetBytes(y));
		}
	}
}
