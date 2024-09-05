// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the MIT License.  See LICENSE in the project root for license information.
using System;

namespace SpanKeyedCollections
{
	/// <summary>
	/// Provides read-write access to a span-keyed dictionary.
	/// </summary>
	/// <typeparam name="TKeyElement">The element type of the key span.</typeparam>
	/// <typeparam name="TValue">The value type.</typeparam>
	public interface ISpanKeyedDictionary<TKeyElement, TValue> : IReadOnlySpanKeyedDictionary<TKeyElement, TValue>
	{
		/// <summary>
		/// Adds the specified key-value pair to the dictionary.
		/// </summary>
		void Add(ReadOnlySpan<TKeyElement> key, TValue value);

		/// <summary>
		/// Removes the specified key and it's associated value from the dictionary.
		/// </summary>
		bool Remove(ReadOnlySpan<TKeyElement> key);
	}
}
