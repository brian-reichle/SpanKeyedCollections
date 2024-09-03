// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the MIT License.  See LICENSE in the project root for license information.
using System;

namespace SpanKeyedCollections
{
	static partial class Primes
	{
		public static int GetPrime(int value)
		{
			var index = Array.BinarySearch(_primes, value);

			if (index >= 0)
			{
				return value;
			}

			index = ~index;

			if (index < _primes.Length)
			{
				return _primes[index];
			}

			return NextPrime(value);
		}

		static int NextPrime(int value)
		{
			// "round up" to the nearest odd number.
			value |= 1;

			while (value < MaxPrime)
			{
				if (IsPrime(value))
				{
					return value;
				}

				value += 2;
			}

			return MaxPrime;
		}

		static bool IsPrime(int value)
		{
			var cap = (int)Math.Sqrt(value);

			for (var i = 2; i < cap; i++)
			{
				if (value % i == 0)
				{
					return false;
				}
			}

			return true;
		}
	}
}
