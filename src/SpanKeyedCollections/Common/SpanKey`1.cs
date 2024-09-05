// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the MIT License.  See LICENSE in the project root for license information.
using System;

namespace SpanKeyedCollections
{
	/// <summary>
	/// A heap-safe wrapper around the internal representation of the key.
	/// </summary>
	/// <typeparam name="TKeyElement">The element type of the key span.</typeparam>
	/// <remarks>
	/// Span keyed dictionaries need to create read-only copies of the passed in spans and be able to provde them to
	/// a consumer upon request without having to worry about the consumer mutating them. This wrapper provides an
	/// easy to get the span without risk of mutation or the heap restrictions of a ref structs.
	/// </remarks>
	public readonly struct SpanKey<TKeyElement>
	{
		public SpanKey(TKeyElement[] data)
		{
			_data = data ?? throw new ArgumentNullException(nameof(data));
		}

		/// <summary>
		/// Returns a ReadOnlySpan&lt;&gt; over the internal representation of the key.
		/// </summary>
		public ReadOnlySpan<TKeyElement> AsSpan() => _data.AsSpan();

		readonly TKeyElement[] _data;
	}
}
