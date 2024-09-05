// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the MIT License.  See LICENSE in the project root for license information.
using System;
using System.Collections;
using System.Collections.Generic;

namespace SpanKeyedCollections
{
	/// <summary>
	/// A hash-set of spans.
	/// </summary>
	/// <typeparam name="TKeyElement">The element type of the spans.</typeparam>
	public sealed class SpanKeyedHashSet<TKeyElement> : IReadOnlyCollection<SpanKey<TKeyElement>>, ICloneable
	{
		/// <summary>
		/// Creates a new SpanKeyedHashSet&lt;&gt; with the provided comparer and starting capacity.
		/// </summary>
		public SpanKeyedHashSet(ISpanEqualityComparer<TKeyElement> comparer, int capacity = DefaultCapacity)
		{
			_equalityComparer = comparer ?? throw new ArgumentNullException(nameof(comparer));

			capacity = Primes.GetPrime(capacity);
			_keys = new TKeyElement[capacity][];
			_hashes = new int[capacity];
			_nextIndexes = new int[capacity];
			_firstIndexes = new int[capacity];
			ResetIndexes(_nextIndexes);
			ResetIndexes(_firstIndexes);
		}

		/// <inheritdoc />
		public int Count => _count;

		/// <summary>
		/// Adds an item specified by the span to the collection.
		/// </summary>
		/// <returns>True if the item was added; False if the item was already in the collection.</returns>
		/// <remarks>
		/// Will not allocate if the item was already in the collection.
		/// </remarks>
		public bool Add(ReadOnlySpan<TKeyElement> item)
		{
			var hash = _equalityComparer.GetHashCode(item);

			if (ContainsCore(item, hash))
			{
				return false;
			}

			AddCore(hash, item.ToArray());
			return true;
		}

		/// <summary>
		/// Removes an item specified by the span.
		/// </summary>
		/// <returns>True if the item was removed; False if the item was not in the collection.</returns>
		/// <remarks>
		/// Will not allocate if the item was not in the collection.
		/// </remarks>
		public bool Remove(ReadOnlySpan<TKeyElement> item)
		{
			var hash = _equalityComparer.GetHashCode(item);
			ref var reference = ref _firstIndexes[Index(hash)];

			while (true)
			{
				if (reference == -1)
				{
					return false;
				}

				if (_hashes[reference] == hash && _equalityComparer.Equals(_keys[reference].AsSpan(), item))
				{
					break;
				}

				reference = ref _nextIndexes[reference];
			}

			var id = reference;
			reference = _nextIndexes[id];
			_count--;

			if (id != _count)
			{
				var lastId = _count;
				FindReference(lastId) = id;
				_hashes[id] = _hashes[lastId];
				_keys[id] = _keys[lastId];
				_nextIndexes[id] = _nextIndexes[lastId];
				id = lastId;
			}

			_hashes[id] = 0;
			_keys[id] = null!;
			_nextIndexes[id] = -1;
			return true;
		}

		/// <summary>
		/// Performs a lookup to specify if the provided value is in the collection.
		/// </summary>
		/// <returns>True if the item was in the collection; False otherwise.</returns>
		public bool Contains(ReadOnlySpan<TKeyElement> item)
			=> ContainsCore(item, _equalityComparer.GetHashCode(item));

		/// <summary>
		/// Creates a copy of the collection.
		/// </summary>
		public SpanKeyedHashSet<TKeyElement> Clone() => new(this);

		/// <inheritdoc />
		public IEnumerator<SpanKey<TKeyElement>> GetEnumerator()
		{
			for (var i = 0; i < _count; i++)
			{
				if (_keys[i] != null)
				{
					yield return new(_keys[i]);
				}
			}
		}

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		object ICloneable.Clone() => Clone();

		static void ResetIndexes(int[] indexes) => ArrayUtils.Fill(indexes, -1);

		SpanKeyedHashSet(SpanKeyedHashSet<TKeyElement> source)
		{
			_equalityComparer = source._equalityComparer;

			// we don't need to clone the individual key arrays as they should be treated as immutable.
			_keys = ArrayUtils.CloneArray(source._keys);
			_hashes = ArrayUtils.CloneArray(source._hashes);
			_nextIndexes = ArrayUtils.CloneArray(source._nextIndexes);
			_firstIndexes = ArrayUtils.CloneArray(source._firstIndexes);
			_count = source._count;
		}

		bool ContainsCore(ReadOnlySpan<TKeyElement> key, int hash)
		{
			var i = _firstIndexes[Index(hash)];

			while (true)
			{
				if (i < 0)
				{
					return false;
				}

				if (_hashes[i] == hash && _equalityComparer.Equals(_keys[i].AsSpan(), key))
				{
					return true;
				}

				i = _nextIndexes[i];
			}
		}

		void AddCore(int hash, TKeyElement[] key)
		{
			if (_count == _firstIndexes.Length)
			{
				GrowCapacity();
			}

			var id = _count;
			_nextIndexes[id] = -1;
			_keys[id] = key;
			_hashes[id] = hash;
			_count++;

			LinkId(hash, id);
		}

		void LinkId(int hash, int id)
		{
			ref var i = ref _firstIndexes[Index(hash)];

			while (true)
			{
				if (i < 0)
				{
					i = id;
					return;
				}

				i = ref _nextIndexes[i];
			}
		}

		ref int FindReference(int id)
		{
			var hash = _hashes[id];
			ref var reference = ref _firstIndexes[Index(hash)];

			while (true)
			{
				if (reference == id)
				{
					return ref reference;
				}
				else if (reference < 0)
				{
					throw new ArgumentException("no reference to provided id was found.");
				}

				reference = ref _nextIndexes[reference];
			}
		}

		void GrowCapacity()
		{
			var newCapacity = Primes.GetPrime((_firstIndexes.Length * 2) + 1);
			Array.Resize(ref _keys, newCapacity);
			Array.Resize(ref _hashes, newCapacity);

			_nextIndexes = new int[newCapacity];
			_firstIndexes = new int[newCapacity];
			ResetIndexes(_nextIndexes);
			ResetIndexes(_firstIndexes);

			for (var i = 0; i < _count; i++)
			{
				LinkId(_hashes[i], i);
			}
		}

		int Index(int hash) => unchecked((int)((uint)hash % (uint)_keys.Length));

		const int DefaultCapacity = 100;

		readonly ISpanEqualityComparer<TKeyElement> _equalityComparer;
		int _count;
		TKeyElement[][] _keys;
		int[] _hashes;
		int[] _nextIndexes;
		int[] _firstIndexes;
	}
}
