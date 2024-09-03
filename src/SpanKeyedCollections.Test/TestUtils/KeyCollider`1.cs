// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the MIT License.  See LICENSE in the project root for license information.
using System;

namespace SpanKeyedCollections.Test
{
	// a comparer that gives everything the same hash code.
	sealed class KeyCollider<T>(ISpanEqualityComparer<T> inner) : ISpanEqualityComparer<T>
		where T : struct
	{
		public bool Equals(ReadOnlySpan<T> x, ReadOnlySpan<T> y) => inner.Equals(x, y);
		public int GetHashCode(ReadOnlySpan<T> obj) => 42;
	}
}
