// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the MIT License.  See LICENSE in the project root for license information.
using System;

namespace SpanKeyedCollections
{
	public interface ISpanKeyedDictionary<TKeyElement, TValue> : IReadOnlySpanKeyedDictionary<TKeyElement, TValue>
	{
		void Add(ReadOnlySpan<TKeyElement> key, TValue value);
		bool Remove(ReadOnlySpan<TKeyElement> key);
	}
}
