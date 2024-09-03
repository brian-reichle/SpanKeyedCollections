// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the MIT License.  See LICENSE in the project root for license information.
using System;

namespace SpanKeyedCollections
{
	public interface ISpanEqualityComparer<T>
		where T : struct
	{
		bool Equals(ReadOnlySpan<T> x, ReadOnlySpan<T> y);
		int GetHashCode(ReadOnlySpan<T> obj);
	}
}
