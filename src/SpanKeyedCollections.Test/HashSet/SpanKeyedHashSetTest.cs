// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the MIT License.  See LICENSE in the project root for license information.
using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SpanKeyedCollections.HashSet;

namespace SpanKeyedCollections.Test
{
	[TestFixture]
	class SpanKeyedHashSetTest
	{
		[Test]
		public void Add([ValueSource(nameof(Comparers))] ISpanEqualityComparer<char> comparer)
		{
			var set = new SpanKeyedHashSet<char>(comparer);

			Assert.Multiple(() =>
			{
				Assert.That(set.Add("Foo".AsSpan()), Is.True, "Adding Foo for the first time.");
				Assert.That(set.Add("Foo".AsSpan()), Is.False, "Already contains Foo.");
				Assert.That(set.Add("Bar".AsSpan()), Is.True, "Adding Bar for the first time.");

				Assert.That(set.Select(x => x.AsSpan().ToString()), Is.EquivalentTo(new[] { "Foo", "Bar" }));
			});
		}

		[Test]
		public void AddExistingShouldNotAllocate()
		{
			var set = new SpanKeyedHashSet<char>(BitwiseComparer)
			{
				"Foo".AsSpan(),
			};

			Assert.Multiple(() =>
			{
				var baseline = GC.GetAllocatedBytesForCurrentThread();
				var result = set.Add("Foo".AsSpan());
				var allocated = GC.GetAllocatedBytesForCurrentThread() - baseline;

				Assert.That(result, Is.False);
				Assert.That(allocated, Is.Zero);
			});
		}

		[Test]
		public void Remove([ValueSource(nameof(Comparers))] ISpanEqualityComparer<char> comparer)
		{
			var set = new SpanKeyedHashSet<char>(comparer)
			{
				"Foo".AsSpan(),
				"Bar".AsSpan(),
				"Baz".AsSpan(),
			};

			Assert.Multiple(() =>
			{
				Assert.That(set.Remove("Bar".AsSpan()), Is.True);
				Assert.That(set.Select(x => x.AsSpan().ToString()), Is.EquivalentTo(new[] { "Foo", "Baz" }));

				Assert.That(set.Remove("Bar".AsSpan()), Is.False);
				Assert.That(set.Select(x => x.AsSpan().ToString()), Is.EquivalentTo(new[] { "Foo", "Baz" }));

				Assert.That(set.Remove("Foo".AsSpan()), Is.True);
				Assert.That(set.Select(x => x.AsSpan().ToString()), Is.EquivalentTo(new[] { "Baz" }));

				Assert.That(set.Remove("Baz".AsSpan()), Is.True);
				Assert.That(set.Select(x => x.AsSpan().ToString()), Is.EquivalentTo(Array.Empty<string>()));
			});
		}

		[Test]
		public void RemoveMissingShouldNotAllocate()
		{
			var set = new SpanKeyedHashSet<char>(BitwiseComparer)
			{
				"Foo".AsSpan(),
			};

			Assert.Multiple(() =>
			{
				var baseline = GC.GetAllocatedBytesForCurrentThread();
				var result = set.Remove("Bar".AsSpan());
				var allocated = GC.GetAllocatedBytesForCurrentThread() - baseline;

				Assert.That(result, Is.False);
				Assert.That(allocated, Is.Zero);
			});
		}

		[Test]
		public void RemoveFromEmpty()
		{
			var set = new SpanKeyedHashSet<char>(BitwiseComparer);
			set.Remove("Foo".AsSpan());

			Assert.That(set, Has.Count.Zero);
		}

		[Test]
		public void Contains([ValueSource(nameof(Comparers))] ISpanEqualityComparer<char> comparer)
		{
			var set = new SpanKeyedHashSet<char>(comparer)
			{
				"Foo".AsSpan(),
				"Bar".AsSpan(),
			};

			Assert.Multiple(() =>
			{
				Assert.That(set.Contains("Foo".AsSpan()), Is.True);
				Assert.That(set.Contains("Bar".AsSpan()), Is.True);
				Assert.That(set.Contains("Baz".AsSpan()), Is.False);
			});
		}

		[Test]
		public void Contains_ShouldNotAllocate([ValueSource(nameof(Comparers))] ISpanEqualityComparer<char> comparer)
		{
			var set = new SpanKeyedHashSet<char>(comparer)
			{
				"Foo".AsSpan(),
				"Bar".AsSpan(),
			};

			var baseline = GC.GetAllocatedBytesForCurrentThread();
			var result = set.Contains("Foo".AsSpan());
			var allocated = GC.GetAllocatedBytesForCurrentThread() - baseline;

			Assert.Multiple(() =>
			{
				Assert.That(result, Is.True);
				Assert.That(allocated, Is.Zero);
			});
		}

		[Test]
		public void Clone()
		{
			var set1 = new SpanKeyedHashSet<char>(BitwiseComparer)
			{
				"Foo".AsSpan(),
				"Bar".AsSpan(),
			};

			var set2 = set1.Clone();

			set1.Add("Baz".AsSpan());
			set2.Remove("Bar".AsSpan());

			Assert.Multiple(() =>
			{
				Assert.That(set1.Select(x => x.AsSpan().ToString()), Is.EquivalentTo(new[] { "Foo", "Bar", "Baz" }));
				Assert.That(set2.Select(x => x.AsSpan().ToString()), Is.EquivalentTo(new[] { "Foo" }));
			});
		}

		protected static IEnumerable<ISpanEqualityComparer<char>> Comparers()
		{
			yield return BitwiseComparer;
			yield return KeyCollider;
		}

		static ISpanEqualityComparer<char> BitwiseComparer => _bitwiseComparer ??= new BitwiseEqualityComparer<char>();
		static ISpanEqualityComparer<char> KeyCollider => _keyCollider ??= new KeyCollider<char>(BitwiseComparer);

		static BitwiseEqualityComparer<char>? _bitwiseComparer;
		static KeyCollider<char>? _keyCollider;
	}
}
