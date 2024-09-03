// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the MIT License.  See LICENSE in the project root for license information.
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace SpanKeyedCollections
{
	public interface IReadOnlySpanKeyedDictionary<TKeyElement, TValue> : IReadOnlyCollection<KeyValuePair<SpanKey<TKeyElement>, TValue>>
	{
		IEnumerable<SpanKey<TKeyElement>> Keys { get; }
		IEnumerable<TValue> Values { get; }

		TValue this[ReadOnlySpan<TKeyElement> key] { get; }

		bool ContainsKey(ReadOnlySpan<TKeyElement> key);
		bool TryGetValue(ReadOnlySpan<TKeyElement> key, [MaybeNullWhen(false)] out TValue value);
	}
}
