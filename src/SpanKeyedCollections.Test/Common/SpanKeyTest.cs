// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the MIT License.  See LICENSE in the project root for license information.
using System;
using NUnit.Framework;

namespace SpanKeyedCollections.Test
{
	[TestFixture]
	class SpanKeyTest
	{
		[Test]
		public void AsSpan()
		{
			var buffer = "text".AsSpan();

			var key = new SpanKey<char>(buffer.ToArray());

			Assert.That(key.AsSpan().Equals(buffer, StringComparison.Ordinal), Is.True);
		}
	}
}
