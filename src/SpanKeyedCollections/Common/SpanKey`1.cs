// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the MIT License.  See LICENSE in the project root for license information.
using System;

namespace SpanKeyedCollections
{
	public readonly struct SpanKey<TKeyElement>
	{
		public SpanKey(TKeyElement[] data)
		{
			_data = data ?? throw new ArgumentNullException(nameof(data));
		}

		public ReadOnlySpan<TKeyElement> AsSpan() => _data.AsSpan();

		readonly TKeyElement[] _data;
	}
}
