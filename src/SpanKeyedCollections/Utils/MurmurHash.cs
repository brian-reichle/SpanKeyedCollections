// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the MIT License.  See LICENSE in the project root for license information.
using System;
using System.Runtime.InteropServices;

namespace SpanKeyedCollections
{
	static class MurmurHash
	{
		public static int Hash(uint seed, ReadOnlySpan<byte> buffer)
		{
			unchecked
			{
				var hash = seed;
				var words = MemoryMarshal.Cast<byte, uint>(buffer);

				for (var i = 0; i < words.Length; i++)
				{
					hash ^= PreProcess(words[i]);
					hash = ROL(hash, R2);
					hash = (hash * M) + N;
				}

				var k = 0u;

				for (var i = buffer.Length & ~0x3; i < buffer.Length; i++)
				{
					k = (k << 8) | buffer[i];
				}

				hash ^= PreProcess(k);
				hash ^= (uint)buffer.Length;
				return (int)PostProcess(hash);
			}
		}

		static uint PostProcess(uint hash)
		{
			unchecked
			{
				hash ^= hash >> 16;
				hash *= 0x85ebca6b;
				hash ^= hash >> 13;
				hash *= 0xc2b2ae35;
				hash ^= hash >> 16;
				return hash;
			}
		}

		static uint PreProcess(uint value) => ROL(value * C1, R1) * C2;
		static uint ROL(uint v, int b) => (v << b) | (v >> (32 - b));

		const uint C1 = 0xcc9e2d51;
		const uint C2 = 0x1b873593;
		const int R1 = 15;
		const int R2 = 13;
		const uint M = 5;
		const uint N = 0xe6546b64;
	}
}
