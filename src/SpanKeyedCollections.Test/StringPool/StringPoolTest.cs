// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the MIT License.  See LICENSE in the project root for license information.
using System;
using NUnit.Framework;

namespace SpanKeyedCollections.Test
{
	[TestFixture]
	class StringPoolTest
	{
		[Test]
		public void GetString_EmptySpan()
		{
			var pool = new StringPool();
			Assert.That(pool.GetString([]), Is.SameAs(string.Empty));
		}

		[Test]
		public void GetString_NewSpan()
		{
			var pool = new StringPool();
			Assert.That(pool.GetString(NewBarSpan()), Is.EqualTo("Bar"));
		}

		[Test]
		public void GetString_NewString()
		{
			var str = "Bar";

			var pool = new StringPool();
			Assert.That(pool.GetString(str), Is.SameAs(str));
		}

		[Test]
		public void GetString_ExistingSpan()
		{
			var pool = new StringPool();
			var existing = pool.GetString(NewBarSpan());

			Assert.Multiple(() =>
			{
				var span = NewBarSpan();
				var baseline = GC.GetAllocatedBytesForCurrentThread();
				var result = pool.GetString(span);
				var allocated = GC.GetAllocatedBytesForCurrentThread() - baseline;

				Assert.That(result, Is.SameAs(existing));
				Assert.That(allocated, Is.Zero);
			});
		}

		[Test]
		public void GetString_ExistingString()
		{
			var pool = new StringPool();
			var existing = pool.GetString(NewBarString());

			Assert.Multiple(() =>
			{
				var str = NewBarString();
				var baseline = GC.GetAllocatedBytesForCurrentThread();
				var result = pool.GetString(str);
				var allocated = GC.GetAllocatedBytesForCurrentThread() - baseline;

				Assert.That(result, Is.SameAs(existing));
				Assert.That(allocated, Is.Zero);
			});
		}

		[Test]
		public void TryGetString_NewSpan()
		{
			var pool = new StringPool();

			Assert.Multiple(() =>
			{
				var span = NewBarSpan();
				var baseline = GC.GetAllocatedBytesForCurrentThread();
				var result = pool.TryGetString(span, out var outString);
				var allocated = GC.GetAllocatedBytesForCurrentThread() - baseline;

				Assert.That(result, Is.False);
				Assert.That(outString, Is.Null);
				Assert.That(allocated, Is.Zero);
			});
		}

		[Test]
		public void TryGetString_NewString()
		{
			var pool = new StringPool();

			Assert.Multiple(() =>
			{
				var baseline = GC.GetAllocatedBytesForCurrentThread();
				var result = pool.TryGetString("Baz", out var outString);
				var allocated = GC.GetAllocatedBytesForCurrentThread() - baseline;

				Assert.That(result, Is.False);
				Assert.That(outString, Is.Null);
				Assert.That(allocated, Is.Zero);
			});
		}

		[Test]
		public void TryGetString_ExistingSpan()
		{
			var pool = new StringPool();
			var existing = pool.GetString(NewBarString());

			Assert.Multiple(() =>
			{
				var span = NewBarSpan();
				var baseline = GC.GetAllocatedBytesForCurrentThread();
				var result = pool.TryGetString(span, out var outString);
				var allocated = GC.GetAllocatedBytesForCurrentThread() - baseline;

				Assert.That(result, Is.True);
				Assert.That(outString, Is.SameAs(existing));
				Assert.That(allocated, Is.Zero);
			});
		}

		[Test]
		public void TrgGetString_ExistingString()
		{
			var pool = new StringPool();
			var existing = pool.GetString(NewBarString());

			Assert.Multiple(() =>
			{
				var str = NewBarString();
				var baseline = GC.GetAllocatedBytesForCurrentThread();
				var result = pool.TryGetString(str, out var outString);
				var allocated = GC.GetAllocatedBytesForCurrentThread() - baseline;

				Assert.That(result, Is.True);
				Assert.That(outString, Is.SameAs(existing));
				Assert.That(allocated, Is.Zero);
			});
		}

		[Test]
		public void MultipleStrings()
		{
			var pool = new StringPool();
			var foo = pool.GetString("Foo");
			var bar = pool.GetString("Bar".AsSpan());
			var baz = pool.GetString("Baz".AsSpan());

			Assert.Multiple(() =>
			{
				Assert.That(foo, Is.Not.EqualTo(bar));
				Assert.That(foo, Is.Not.EqualTo(baz));
				Assert.That(bar, Is.Not.EqualTo(baz));

				Assert.That(pool.GetString("Foo".AsSpan()), Is.SameAs(foo));
				Assert.That(pool.GetString("Bar"), Is.SameAs(bar));
				Assert.That(pool.GetString("Baz".AsSpan()), Is.SameAs(baz));
			});
		}

		[Test]
		public void GrowCapacity()
		{
			var pool = new StringPool(3);
			pool.GetString("A");
			pool.GetString("B");
			pool.GetString("C");
			pool.GetString("D");

			Assert.Multiple(() =>
			{
				Assert.That(pool.TryGetString("A", out _), Is.True);
				Assert.That(pool.TryGetString("B", out _), Is.True);
				Assert.That(pool.TryGetString("C", out _), Is.True);
				Assert.That(pool.TryGetString("D", out _), Is.True);
				Assert.That(pool.TryGetString("E", out _), Is.False);
			});
		}

		[Test]
		public void Clone()
		{
			var pool1 = new StringPool();
			pool1.GetString(Text(0));
			pool1.GetString(Text(1));

			var pool2 = pool1.Clone();
			pool2.GetString(Text(2));
			pool1.GetString(Text(3));

			Assert.Multiple(() =>
			{
				Assert.That(pool2.GetString(Text(0)), Is.SameAs(pool1.GetString(Text(0))), "Cloned pool should use the same string instances (0).");
				Assert.That(pool2.GetString(Text(1)), Is.SameAs(pool1.GetString(Text(1))), "Cloned pool should use the same string instances (1).");

				Assert.That(pool1.TryGetString(Text(2), out _), Is.False, "Text(2) was not added to pool1.");
				Assert.That(pool2.TryGetString(Text(2), out _), Is.True, "Text(2) was added to pool2.");

				Assert.That(pool1.TryGetString(Text(3), out _), Is.True, "Text(3) was added to pool1.");
				Assert.That(pool2.TryGetString(Text(3), out _), Is.False, "Text(3) was not added to pool1.");
			});

			static ReadOnlySpan<char> Text(int i) => "FooBarBazQux".AsSpan(i * 3, 3);
		}

		static ReadOnlySpan<char> NewBarSpan() => ['B', 'a', 'r'];
		static string NewBarString() => new(['B', 'a', 'r']);
	}
}
