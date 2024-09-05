// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the MIT License.  See LICENSE in the project root for license information.
using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace SpanKeyedCollections.Test
{
	[TestFixture]
	class SpanKeyedDictionaryTest
	{
		[Test]
		public void Add([ValueSource(nameof(Comparers))] ISpanEqualityComparer<char> comparer)
		{
			var dictionary = new SpanKeyedDictionary<char, int>(comparer);
			Assert.That(dictionary, Has.Count.EqualTo(0));

			dictionary.Add("Foo".AsSpan(), 7);
			Assert.That(dictionary, Has.Count.EqualTo(1));

			dictionary.Add("Bar".AsSpan(), 42);
			Assert.That(dictionary, Has.Count.EqualTo(2));
		}

		[Test]
		public void Remove([ValueSource(nameof(Comparers))] ISpanEqualityComparer<char> comparer)
		{
			var dictionary = new SpanKeyedDictionary<char, int>(comparer);
			Assert.That(dictionary, Has.Count.EqualTo(0));

			dictionary.Add("Foo".AsSpan(), 1);
			dictionary.Add("Bar".AsSpan(), 2);
			dictionary.Add("Baz".AsSpan(), 3);
			dictionary.Add("Qux".AsSpan(), 4);
			dictionary.Add("Quux".AsSpan(), 5);
			dictionary.Add("Quuz".AsSpan(), 6);

			AssertRemove("xyzzy", success: false, ["Foo", "Bar", "Baz", "Qux", "Quux", "Quuz"]);
			AssertRemove("Quuz", success: true, ["Foo", "Bar", "Baz", "Qux", "Quux"]);
			AssertRemove("Baz", success: true, ["Foo", "Bar", "Qux", "Quux"]);
			AssertRemove("Baz", success: false, ["Foo", "Bar", "Qux", "Quux"]);
			AssertRemove("Foo", success: true, ["Bar", "Qux", "Quux"]);

			Assert.Multiple(() =>
			{
				Assert.That(dictionary.ContainsKey("Foo".AsSpan()), Is.False);
				Assert.That(dictionary.ContainsKey("Bar".AsSpan()), Is.True);
				Assert.That(dictionary.ContainsKey("Baz".AsSpan()), Is.False);
				Assert.That(dictionary.ContainsKey("Qux".AsSpan()), Is.True);
				Assert.That(dictionary.ContainsKey("Quux".AsSpan()), Is.True);
				Assert.That(dictionary.ContainsKey("Quuz".AsSpan()), Is.False);
			});

			void AssertRemove(string key, bool success, string[] expected)
			{
				Assert.Multiple(() =>
				{
					Assert.That(dictionary.Remove(key.AsSpan()), Is.EqualTo(success));
					Assert.That(Keys(dictionary), Is.EquivalentTo(expected));
					Assert.That(dictionary.Keys.Select(x => x.AsSpan().ToString()), Is.EquivalentTo(expected));
				});
			}
		}

		[Test]
		public void ContainsKey()
		{
			var dictionary = new SpanKeyedDictionary<char, int>(BitwiseComparer)
			{
				{ "Foo".AsSpan(), 7 },
				{ "Bar".AsSpan(), 42 },
			};

			Assert.Multiple(() =>
			{
				Assert.That(dictionary.ContainsKey("Foo".AsSpan()), Is.True);
				Assert.That(dictionary.ContainsKey("Bar".AsSpan()), Is.True);
				Assert.That(dictionary.ContainsKey("Baz".AsSpan()), Is.False);
			});
		}

		[Test]
		public void TryGet([ValueSource(nameof(Comparers))] ISpanEqualityComparer<char> comparer)
		{
			var dictionary = new SpanKeyedDictionary<char, int>(comparer)
			{
				{ "Foo".AsSpan(), 7 },
				{ "Bar".AsSpan(), 42 },
			};

			Assert.Multiple(() =>
			{
				Assert.That(dictionary.TryGetValue("Foo".AsSpan(), out var value), Is.True);
				Assert.That(value, Is.EqualTo(7));

				Assert.That(dictionary.TryGetValue("Bar".AsSpan(), out value), Is.True);
				Assert.That(value, Is.EqualTo(42));

				Assert.That(dictionary.TryGetValue("Baz".AsSpan(), out value), Is.False);
				Assert.That(value, Is.EqualTo(0));
			});
		}

		[Test]
		public void TryGet_ShouldNotAllocate()
		{
			var dictionary = new SpanKeyedDictionary<char, int>(BitwiseComparer)
			{
				{ "Foo".AsSpan(), 7 },
				{ "Bar".AsSpan(), 42 },
			};

			Assert.Multiple(() =>
			{
				var baseline = GC.GetAllocatedBytesForCurrentThread();
				var result = dictionary.TryGetValue("Foo".AsSpan(), out var value);
				var allocated = GC.GetAllocatedBytesForCurrentThread() - baseline;

				Assert.That(result, Is.True);
				Assert.That(value, Is.EqualTo(7));
				Assert.That(allocated, Is.Zero);
			});
		}

		[Test]
		public void IndexerGet()
		{
			var dictionary = new SpanKeyedDictionary<char, int>(BitwiseComparer)
			{
				{ "Foo".AsSpan(), 7 },
				{ "Bar".AsSpan(), 42 },
			};

			Assert.Multiple(() =>
			{
				Assert.That(dictionary["Foo".AsSpan()], Is.EqualTo(7));
				Assert.That(dictionary["Bar".AsSpan()], Is.EqualTo(42));
				Assert.That(() => dictionary["Baz".AsSpan()], Throws.InstanceOf<IndexOutOfRangeException>());
			});
		}

		[Test]
		public void IndexerSet()
		{
			var dictionary = new SpanKeyedDictionary<char, int>(BitwiseComparer)
			{
				{ "Foo".AsSpan(), 7 },
				{ "Bar".AsSpan(), 42 },
			};

			dictionary["Foo".AsSpan()] = 13;
			dictionary["Baz".AsSpan()] = 17;

			Assert.Multiple(() =>
			{
				Assert.That(dictionary["Foo".AsSpan()], Is.EqualTo(13));
				Assert.That(dictionary["Bar".AsSpan()], Is.EqualTo(42));
				Assert.That(dictionary["Baz".AsSpan()], Is.EqualTo(17));
			});
		}

		[Test]
		public void Enumerate()
		{
			var dictionary = new SpanKeyedDictionary<char, int>(BitwiseComparer)
			{
				{ "Foo".AsSpan(), 7 },
				{ "Bar".AsSpan(), 42 },
			};

			var expected = new[]
			{
				new KeyValuePair<string, int>("Foo", 7),
				new KeyValuePair<string, int>("Bar", 42),
			};

			Assert.That(AsKeyValuePair(dictionary), Is.EquivalentTo(expected));
		}

		[Test]
		public void EnumerateKeys()
		{
			var dictionary = new SpanKeyedDictionary<char, int>(BitwiseComparer)
			{
				{ "Foo".AsSpan(), 7 },
				{ "Bar".AsSpan(), 42 },
			};

			var expected = new[]
			{
				"Foo",
				"Bar",
			};

			Assert.That(dictionary.Keys.Select(x => x.AsSpan().ToString()), Is.EquivalentTo(expected));
		}

		[Test]
		public void EnumerateValues()
		{
			var dictionary = new SpanKeyedDictionary<char, int>(BitwiseComparer)
			{
				{ "Foo".AsSpan(), 7 },
				{ "Bar".AsSpan(), 42 },
			};

			var expected = new[]
			{
				7,
				42,
			};

			Assert.That(dictionary.Values, Is.EquivalentTo(expected));
		}

		[Test]
		public void Grow()
		{
			var dictionary = new SpanKeyedDictionary<char, int>(BitwiseComparer, capacity: 3);
			Span<char> buffer = stackalloc char[3];

			for (var i = 0; i < 24; i++)
			{
				buffer[0] = (char)('A' + i);
				buffer[1] = (char)('A' + i + 1);
				buffer[2] = (char)('A' + i + 2);
				dictionary.Add(buffer, i);
			}

			Assert.Multiple(() =>
			{
				Assert.That(dictionary, Has.Count.EqualTo(24));
				Assert.That(dictionary["ABC".AsSpan()], Is.EqualTo(0));
				Assert.That(dictionary["XYZ".AsSpan()], Is.EqualTo(23));
			});
		}

		[Test]
		public void Clone()
		{
			var dictionary1 = new SpanKeyedDictionary<char, int>(BitwiseComparer)
			{
				{ "Foo".AsSpan(), 7 },
				{ "Bar".AsSpan(), 42 },
			};

			var dictionary2 = dictionary1.Clone();

			dictionary1["Foo".AsSpan()] = -7;
			dictionary2["Bar".AsSpan()] = -42;

			Assert.Multiple(() =>
			{
				Assert.That(dictionary2, Has.Count.EqualTo(2));
				Assert.That(dictionary1["Foo".AsSpan()], Is.EqualTo(-7));
				Assert.That(dictionary1["Bar".AsSpan()], Is.EqualTo(42));
				Assert.That(dictionary2["Foo".AsSpan()], Is.EqualTo(7));
				Assert.That(dictionary2["Bar".AsSpan()], Is.EqualTo(-42));
			});
		}

		protected static IEnumerable<ISpanEqualityComparer<char>> Comparers()
		{
			yield return BitwiseComparer;
			yield return KeyCollider;
		}

		static IEnumerable<KeyValuePair<string, TValue>> AsKeyValuePair<TValue>(IEnumerable<KeyValuePair<SpanKey<char>, TValue>> source)
			=> source.Select(x => new KeyValuePair<string, TValue>(x.Key.AsSpan().ToString(), x.Value));

		static IEnumerable<string> Keys<TValue>(IEnumerable<KeyValuePair<SpanKey<char>, TValue>> source)
			=> source.Select(x => x.Key.AsSpan().ToString());

		static ISpanEqualityComparer<char> BitwiseComparer => _bitwiseComparer ??= new BitwiseEqualityComparer<char>();
		static ISpanEqualityComparer<char> KeyCollider => _keyCollider ??= new KeyCollider<char>(BitwiseComparer);

		static BitwiseEqualityComparer<char>? _bitwiseComparer;
		static KeyCollider<char>? _keyCollider;
	}
}
