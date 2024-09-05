// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the MIT License.  See LICENSE in the project root for license information.
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace SpanKeyedCollections
{
	/// <summary>
	/// Provides read-only access to a span-keyed dictionary.
	/// </summary>
	/// <typeparam name="TKeyElement">The element type of the key span.</typeparam>
	/// <typeparam name="TValue">The value type.</typeparam>
	public interface IReadOnlySpanKeyedDictionary<TKeyElement, TValue> : IReadOnlyCollection<KeyValuePair<SpanKey<TKeyElement>, TValue>>
	{
		/// <summary>
		/// Gets a collection containing the keys in the dictionary.
		/// </summary>
		IEnumerable<SpanKey<TKeyElement>> Keys { get; }

		/// <summary>
		/// Gets a collection containing the values in the dictionary.
		/// </summary>
		IEnumerable<TValue> Values { get; }

		/// <summary>
		/// Allows the value for a specified key to be set or fetched.
		/// </summary>
		TValue this[ReadOnlySpan<TKeyElement> key] { get; }

		/// <summary>
		/// Performs a lookup to see if the prvided key exists in the dictionary.
		/// </summary>
		bool ContainsKey(ReadOnlySpan<TKeyElement> key);

		/// <summary>
		/// Performs a lookup to see if the provided key exists in the dictionary,
		/// and if it does, retrieves the value.
		/// </summary>
		bool TryGetValue(ReadOnlySpan<TKeyElement> key, [MaybeNullWhen(false)] out TValue value);
	}
}
