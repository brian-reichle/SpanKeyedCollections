// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the MIT License.  See LICENSE in the project root for license information.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace SpanKeyedCollections
{
	/// <summary>
	/// A dictionary that allows ReadOnlySpan&lt;&gt;s to be used as the key.
	/// </summary>
	/// <typeparam name="TKeyElement">The element type of the key span.</typeparam>
	/// <typeparam name="TValue">The value type.</typeparam>
	public sealed class SpanKeyedDictionary<TKeyElement, TValue> : ISpanKeyedDictionary<TKeyElement, TValue>, ICloneable
	{
		/// <summary>
		/// Creates a new SpanKeyedDictionary with the provided comparer and capacity.
		/// </summary>
		public SpanKeyedDictionary(ISpanEqualityComparer<TKeyElement> comparer, int capacity = DefaultCapacity)
		{
			_equalityComparer = comparer ?? throw new ArgumentNullException(nameof(comparer));

			capacity = Primes.GetPrime(capacity);
			_keys = new TKeyElement[capacity][];
			_values = new TValue[capacity];
			_hashes = new int[capacity];
			_nextIndexes = new int[capacity];
			_firstIndexes = new int[capacity];
			ResetIndexes(_nextIndexes);
			ResetIndexes(_firstIndexes);
		}

		/// <inheritdoc />
		public int Count => _count;

		/// <inheritdoc />
		public IEnumerable<SpanKey<TKeyElement>> Keys => new KeyCollection(this);

		/// <inheritdoc />
		public IEnumerable<TValue> Values => new ValueCollection(this);

		/// <inheritdoc />
		public TValue this[ReadOnlySpan<TKeyElement> key]
		{
			get
			{
				if (!TryGetValue(key, out var value))
				{
					throw new IndexOutOfRangeException();
				}

				return value;
			}
			set
			{
				var hash = _equalityComparer.GetHashCode(key);
				SetCore(hash, key, value);
			}
		}

		/// <inheritdoc />
		public void Add(ReadOnlySpan<TKeyElement> key, TValue value)
		{
			var hash = _equalityComparer.GetHashCode(key);

			if (TryGetCore(key, hash, out _))
			{
				throw new InvalidOperationException("Key already exists in dictionary.");
			}

			AddCore(hash, key.ToArray(), value);
		}

		/// <inheritdoc />
		public bool Remove(ReadOnlySpan<TKeyElement> key)
		{
			var hash = _equalityComparer.GetHashCode(key);
			ref var reference = ref _firstIndexes[Index(hash)];

			while (true)
			{
				if (reference == -1)
				{
					return false;
				}

				if (_hashes[reference] == hash && _equalityComparer.Equals(_keys[reference].AsSpan(), key))
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
				_values[id] = _values[lastId];
				_nextIndexes[id] = _nextIndexes[lastId];
				id = lastId;
			}

			_hashes[id] = 0;
			_keys[id] = null!;
			_values[id] = default!;
			_nextIndexes[id] = -1;
			return true;
		}

		/// <inheritdoc />
		public bool ContainsKey(ReadOnlySpan<TKeyElement> key)
			=> TryGetValue(key, out _);

		/// <inheritdoc />
		public bool TryGetValue(ReadOnlySpan<TKeyElement> key, [MaybeNullWhen(false)] out TValue value)
			=> TryGetCore(key, _equalityComparer.GetHashCode(key), out value);

		/// <summary>
		/// Creates a new copy of the dictionary.
		/// </summary>
		public SpanKeyedDictionary<TKeyElement, TValue> Clone() => new(this);

		/// <inheritdoc />
		public IEnumerator<KeyValuePair<SpanKey<TKeyElement>, TValue>> GetEnumerator()
		{
			for (var i = 0; i < _count; i++)
			{
				if (_keys[i] != null)
				{
					yield return new(new(_keys[i]), _values[i]);
				}
			}
		}

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		object ICloneable.Clone() => Clone();

		static void ResetIndexes(int[] indexes) => ArrayUtils.Fill(indexes, -1);

		IEnumerator<SpanKey<TKeyElement>> GetKeyEnumerator()
		{
			for (var i = 0; i < _count; i++)
			{
				if (_keys[i] != null)
				{
					yield return new(_keys[i]);
				}
			}
		}

		IEnumerator<TValue> GetValueEnumerator()
		{
			for (var i = 0; i < _count; i++)
			{
				if (_keys[i] != null)
				{
					yield return _values[i];
				}
			}
		}

		SpanKeyedDictionary(SpanKeyedDictionary<TKeyElement, TValue> source)
		{
			_equalityComparer = source._equalityComparer;

			// we don't need to clone the individual key arrays as they should be treated as immutable.
			_keys = ArrayUtils.CloneArray(source._keys);
			_values = ArrayUtils.CloneArray(source._values);
			_hashes = ArrayUtils.CloneArray(source._hashes);
			_nextIndexes = ArrayUtils.CloneArray(source._nextIndexes);
			_firstIndexes = ArrayUtils.CloneArray(source._firstIndexes);

			_count = source._count;
		}

		bool TryGetCore(ReadOnlySpan<TKeyElement> key, int hash, [MaybeNullWhen(false)] out TValue result)
		{
			var i = _firstIndexes[Index(hash)];

			while (true)
			{
				if (i < 0)
				{
					result = default;
					return false;
				}

				if (_hashes[i] == hash && _equalityComparer.Equals(_keys[i].AsSpan(), key))
				{
					result = _values[i]!;
					return true;
				}

				i = _nextIndexes[i];
			}
		}

		void SetCore(int hash, ReadOnlySpan<TKeyElement> key, TValue value)
		{
			var i = _firstIndexes[Index(hash)];

			while (true)
			{
				if (i < 0)
				{
					AddCore(hash, key.ToArray(), value);
					return;
				}

				if (_hashes[i] == hash && _equalityComparer.Equals(_keys[i].AsSpan(), key))
				{
					_values[i] = value;
					return;
				}

				i = _nextIndexes[i];
			}
		}

		void AddCore(int hash, TKeyElement[] key, TValue value)
		{
			if (_count == _firstIndexes.Length)
			{
				GrowCapacity();
			}

			var id = _count;
			_nextIndexes[id] = -1;
			_keys[id] = key;
			_values[id] = value;
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
			Array.Resize(ref _values, newCapacity);
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
		TValue[] _values;
		int[] _hashes;
		int[] _nextIndexes;
		int[] _firstIndexes;

		sealed class KeyCollection(SpanKeyedDictionary<TKeyElement, TValue> dictionary) : IReadOnlyCollection<SpanKey<TKeyElement>>
		{
			public int Count => dictionary.Count;
			public IEnumerator<SpanKey<TKeyElement>> GetEnumerator() => dictionary.GetKeyEnumerator();
			IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
		}

		sealed class ValueCollection(SpanKeyedDictionary<TKeyElement, TValue> dictionary) : IReadOnlyCollection<TValue>
		{
			public int Count => dictionary.Count;
			public IEnumerator<TValue> GetEnumerator() => dictionary.GetValueEnumerator();
			IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
		}
	}
}
