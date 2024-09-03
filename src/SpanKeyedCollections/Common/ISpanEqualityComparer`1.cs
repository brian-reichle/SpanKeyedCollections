// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the MIT License.  See LICENSE in the project root for license information.
using System;

namespace SpanKeyedCollections
{
	public interface ISpanEqualityComparer<T>
	{
		bool Equals(ReadOnlySpan<T> x, ReadOnlySpan<T> y);
		int GetHashCode(ReadOnlySpan<T> obj);
	}
}
