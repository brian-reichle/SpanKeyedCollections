// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the MIT License.  See LICENSE in the project root for license information.
using System;
using System.Runtime.InteropServices;

namespace SpanKeyedCollections
{
	public sealed class BitwiseEqualityComparer<TKeyElement> : ISpanEqualityComparer<TKeyElement>
		where TKeyElement : unmanaged
	{
		public BitwiseEqualityComparer()
			: this(Environment.TickCount)
		{
		}

		public BitwiseEqualityComparer(int seed)
		{
			_seed = unchecked((uint)seed);
		}

		public bool Equals(ReadOnlySpan<TKeyElement> x, ReadOnlySpan<TKeyElement> y)
			=> Blittable.BitwiseEqual(x, y);

		public int GetHashCode(ReadOnlySpan<TKeyElement> obj)
			=> MurmurHash.Hash(_seed, MemoryMarshal.AsBytes(obj));

		readonly uint _seed;
	}
}
