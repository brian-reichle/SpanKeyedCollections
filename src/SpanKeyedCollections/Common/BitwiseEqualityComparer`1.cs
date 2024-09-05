// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the MIT License.  See LICENSE in the project root for license information.
using System;
using System.Runtime.InteropServices;

namespace SpanKeyedCollections
{
	/// <summary>
	/// Compares spans by looking at the raw bytes occupied by the span.
	/// </summary>
	/// <typeparam name="T">The element type of the spans to compare.</typeparam>
	public sealed class BitwiseEqualityComparer<T> : ISpanEqualityComparer<T>
		where T : unmanaged
	{
		/// <summary>
		/// Create a new BitwiseEqualityComparer&lt;T&gt;.
		/// </summary>
		public BitwiseEqualityComparer()
			: this(Environment.TickCount)
		{
		}

		/// <summary>
		/// Create a new BitwiseEqualityComparer&lt;T&gt; with the provided seed to use in hashing.
		/// </summary>
		public BitwiseEqualityComparer(int seed)
		{
			_seed = unchecked((uint)seed);
		}

		/// <inheritdoc />
		public bool Equals(ReadOnlySpan<T> x, ReadOnlySpan<T> y)
			=> Blittable.BitwiseEqual(x, y);

		/// <inheritdoc />
		public int GetHashCode(ReadOnlySpan<T> obj)
			=> MurmurHash.Hash(_seed, MemoryMarshal.AsBytes(obj));

		readonly uint _seed;
	}
}
