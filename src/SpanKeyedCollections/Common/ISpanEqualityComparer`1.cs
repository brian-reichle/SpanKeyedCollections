// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the MIT License.  See LICENSE in the project root for license information.
using System;

namespace SpanKeyedCollections
{
	/// <summary>
	/// Defines methods to support the comparison of spans for equality.
	/// </summary>
	/// <typeparam name="T">The element type of the spans to compare.</typeparam>
	public interface ISpanEqualityComparer<T>
	{
		/// <summary>
		/// Compares the two provided spans for equality.
		/// </summary>
		/// <returns>
		/// True if the spans are considered equal; otherwise false.
		/// </returns>
		bool Equals(ReadOnlySpan<T> x, ReadOnlySpan<T> y);

		/// <summary>
		/// Calculates a hash code from the provided span.
		/// </summary>
		/// <returns>
		/// Returns the calculated hash code. The same comparer will always return the same hash code for the same input.
		/// </returns>
		int GetHashCode(ReadOnlySpan<T> obj);
	}
}
