// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the MIT License.  See LICENSE in the project root for license information.
using System;
using System.Runtime.InteropServices;

namespace SpanKeyedCollections
{
	static class Blittable
	{
		public static bool BitwiseEqual<T>(ReadOnlySpan<T> x, ReadOnlySpan<T> y)
			where T : unmanaged
			=> BitwiseEqual(MemoryMarshal.AsBytes(x), MemoryMarshal.AsBytes(y));

		public static bool BitwiseEqual(ReadOnlySpan<byte> x, ReadOnlySpan<byte> y)
		{
			if (x.Length != y.Length)
			{
				return false;
			}

			var xw = MemoryMarshal.Cast<byte, int>(x);
			var yw = MemoryMarshal.Cast<byte, int>(y);

			for (var i = 0; i < xw.Length; i++)
			{
				if (xw[i] != yw[i])
				{
					return false;
				}
			}

			for (var i = x.Length & ~3; i < x.Length; i++)
			{
				if (x[i] != y[i])
				{
					return false;
				}
			}

			return true;
		}
	}
}
