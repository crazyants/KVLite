﻿// File name: AbstractCacheTests.cs
// 
// Author(s): Alessio Parma <alessio.parma@gmail.com>
// 
// The MIT License (MIT)
// 
// Copyright (c) 2014-2015 Alessio Parma <alessio.parma@gmail.com>
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and
// associated documentation files (the "Software"), to deal in the Software without restriction,
// including without limitation the rights to use, copy, modify, merge, publish, distribute,
// sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or
// substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT
// NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,
// DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Finsa.CodeServices.Clock;
using FSharpx;
using Microsoft.FSharp.Collections;
using Microsoft.FSharp.Core;
using Ninject;
using NUnit.Framework;
using PommaLabs.KVLite;
using PommaLabs.KVLite.Utilities.Extensions;
using PommaLabs.KVLite.Utilities.Testing;
using Task = System.Threading.Tasks.Task;

namespace UnitTests
{
    [TestFixture]
    internal abstract class AbstractCacheTests
    {
        #region Setup/Teardown

        protected ICache Cache;

        [SetUp]
        public virtual void SetUp()
        {
            Cache.Clear();
        }

        [TearDown]
        public virtual void TearDown()
        {
            Cache.Clear();
            Cache = null;
        }

        #endregion Setup/Teardown

        #region Constants

        private const int LargeItemCount = 1000;

        private const int MediumItemCount = 100;

        private const int SmallItemCount = 10;

        private const int MinItem = 10000;

        protected readonly List<string> StringItems = Enumerable
            .Range(MinItem, LargeItemCount)
            .Select(x => x.ToString(CultureInfo.InvariantCulture))
            .ToList();

        protected readonly IKernel Kernel = new StandardKernel(new NinjectConfig());

        #endregion Constants

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void AddSliding_NullKey()
        {
            Cache.AddSliding(null, StringItems[1], TimeSpan.FromSeconds(10));
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void AddSliding_NullPartition()
        {
            Cache.AddSliding(null, StringItems[0], StringItems[1], TimeSpan.FromSeconds(10));
        }

        [Test]
        public void AddSliding_RightInfo()
        {
            var p = StringItems[0];
            var k = StringItems[1];
            var v1 = StringItems[2];
            var v2 = StringItems[3];
            var i = TimeSpan.FromMinutes(10);
            Cache.AddSliding(p, k, Tuple.Create(v1, v2), i);
            var info = Cache.GetItem<Tuple<string, string>>(p, k).Value;
            Assert.IsNotNull(info);
            Assert.AreEqual(p, info.Partition);
            Assert.AreEqual(k, info.Key);
            Assert.AreEqual(v1, info.Value.Item1);
            Assert.AreEqual(v2, info.Value.Item2);
            Assert.IsNotNull(info.UtcExpiry);
            Assert.AreEqual(i, info.Interval);
        }

        [Test]
        public void AddSlidingAsync_RightInfo()
        {
            var p = StringItems[0];
            var k = StringItems[1];
            var v1 = StringItems[2];
            var v2 = StringItems[3];
            var i = TimeSpan.FromMinutes(10);
            Cache.AddSlidingAsync(p, k, Tuple.Create(v1, v2), i).Wait();
            var info = Cache.GetItem<Tuple<string, string>>(p, k).Value;
            Assert.IsNotNull(info);
            Assert.AreEqual(p, info.Partition);
            Assert.AreEqual(k, info.Key);
            Assert.AreEqual(v1, info.Value.Item1);
            Assert.AreEqual(v2, info.Value.Item2);
            Assert.IsNotNull(info.UtcExpiry);
            Assert.AreEqual(i, info.Interval);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void AddStatic_NullKey()
        {
            Cache.AddStatic(null, StringItems[1]);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void AddStatic_NullPartition()
        {
            Cache.AddStatic(null, StringItems[0], StringItems[1]);
        }

        [Test]
        public void AddStatic_NullValue()
        {
            Cache.AddStatic(StringItems[0], (object) null);
        }

        [Test]
        public void AddStatic_RightInfo()
        {
            var p = StringItems[0];
            var k = StringItems[1];
            var v1 = StringItems[2];
            var v2 = StringItems[3];
            Cache.AddStatic(p, k, Tuple.Create(v1, v2));
            var info = Cache.GetItem<Tuple<string, string>>(p, k).Value;
            Assert.IsNotNull(info);
            Assert.AreEqual(p, info.Partition);
            Assert.AreEqual(k, info.Key);
            Assert.AreEqual(v1, info.Value.Item1);
            Assert.AreEqual(v2, info.Value.Item2);
            Assert.IsNotNull(info.UtcExpiry);
            Assert.AreEqual(TimeSpan.FromDays(Cache.Settings.StaticIntervalInDays), info.Interval);
        }

        [Test]
        public void AddStaticAsync_RightInfo()
        {
            var p = StringItems[0];
            var k = StringItems[1];
            var v1 = StringItems[2];
            var v2 = StringItems[3];
            Cache.AddStaticAsync(p, k, Tuple.Create(v1, v2)).Wait();
            var info = Cache.GetItem<Tuple<string, string>>(p, k).Value;
            Assert.IsNotNull(info);
            Assert.AreEqual(p, info.Partition);
            Assert.AreEqual(k, info.Key);
            Assert.AreEqual(v1, info.Value.Item1);
            Assert.AreEqual(v2, info.Value.Item2);
            Assert.IsNotNull(info.UtcExpiry);
            Assert.AreEqual(TimeSpan.FromDays(Cache.Settings.StaticIntervalInDays), info.Interval);
        }

        [Test]
        public void AddTimed_HugeValue()
        {
            var k = StringItems[1];
            var v = new byte[20000];
            Cache.AddTimed(k, v, Cache.Clock.UtcNow.AddMinutes(10));
            var info = Cache.GetItem<byte[]>(k).Value;
            Assert.IsNotNull(info);
            Assert.AreEqual(k, info.Key);
            Assert.AreEqual(v, info.Value);
            Assert.IsNotNull(info.UtcExpiry);
            Assert.IsNull(info.Interval);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void AddTimed_NullKey()
        {
            Cache.AddTimed(null, StringItems[1], Cache.Clock.UtcNow);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void AddTimed_NullPartition()
        {
            Cache.AddTimed(null, StringItems[0], StringItems[1], Cache.Clock.UtcNow);
        }

        [Test]
        public void AddTimed_RightInfo()
        {
            var p = StringItems[0];
            var k = StringItems[1];
            var v1 = StringItems[2];
            var v2 = StringItems[3];
            var e = Cache.Clock.UtcNow.AddMinutes(10);
            Cache.AddTimed(p, k, Tuple.Create(v1, v2), e);
            var info = Cache.GetItem<Tuple<string, string>>(p, k).Value;
            Assert.IsNotNull(info);
            Assert.AreEqual(p, info.Partition);
            Assert.AreEqual(k, info.Key);
            Assert.AreEqual(v1, info.Value.Item1);
            Assert.AreEqual(v2, info.Value.Item2);

            Assert.IsNotNull(info.UtcExpiry);
            Assert.AreEqual(e.Date, info.UtcExpiry.Date);
            Assert.AreEqual(e.Hour, info.UtcExpiry.Hour);
            Assert.AreEqual(e.Minute, info.UtcExpiry.Minute);
            Assert.AreEqual(e.Second, info.UtcExpiry.Second);

            Assert.IsNull(info.Interval);
        }

        [Test]
        public void AddTimedAsync_RightInfo()
        {
            var p = StringItems[0];
            var k = StringItems[1];
            var v1 = StringItems[2];
            var v2 = StringItems[3];
            var e = Cache.Clock.UtcNow.AddMinutes(10);
            Cache.AddTimedAsync(p, k, Tuple.Create(v1, v2), e).Wait();
            var info = Cache.GetItem<Tuple<string, string>>(p, k).Value;
            Assert.IsNotNull(info);
            Assert.AreEqual(p, info.Partition);
            Assert.AreEqual(k, info.Key);
            Assert.AreEqual(v1, info.Value.Item1);
            Assert.AreEqual(v2, info.Value.Item2);

            Assert.IsNotNull(info.UtcExpiry);
            Assert.AreEqual(e.Date, info.UtcExpiry.Date);
            Assert.AreEqual(e.Hour, info.UtcExpiry.Hour);
            Assert.AreEqual(e.Minute, info.UtcExpiry.Minute);
            Assert.AreEqual(e.Second, info.UtcExpiry.Second);

            Assert.IsNull(info.Interval);
        }

        [TestCase(SmallItemCount)]
        [TestCase(MediumItemCount)]
        [TestCase(LargeItemCount)]
        public void AddTimed_TwoTimes(int itemCount)
        {
            for (var i = 0; i < itemCount; ++i)
            {
                Cache.AddTimed(StringItems[i], StringItems[i], Cache.Clock.UtcNow.Add(TimeSpan.FromMinutes(10)));
                Assert.True(Cache.Contains(StringItems[i]));
            }
            for (var i = 0; i < itemCount; ++i)
            {
                Cache.AddTimed(StringItems[i], StringItems[i], Cache.Clock.UtcNow.Add(TimeSpan.FromMinutes(10)));
                Assert.True(Cache.Contains(StringItems[i]));
            }
        }

        [TestCase(SmallItemCount)]
        [TestCase(MediumItemCount)]
        [TestCase(LargeItemCount)]
        public void AddTimed_TwoTimes_CheckItems(int itemCount)
        {
            for (var i = 0; i < itemCount; ++i)
            {
                Cache.AddTimed(StringItems[i], StringItems[i], Cache.Clock.UtcNow.Add(TimeSpan.FromMinutes(10)));
                Assert.True(Cache.Contains(StringItems[i]));
            }
            for (var i = 0; i < itemCount; ++i)
            {
                Cache.AddTimed(StringItems[i], StringItems[i], Cache.Clock.UtcNow.Add(TimeSpan.FromMinutes(10)));
                Assert.True(Cache.Contains(StringItems[i]));
            }
            var items = Cache.GetItems<string>();
            for (var i = 0; i < itemCount; ++i)
            {
                var s = StringItems[i];
                Assert.True(items.Count(x => x.Key == s && x.Value == s) == 1);
            }
        }

        [TestCase(SmallItemCount)]
        [TestCase(MediumItemCount)]
        [TestCase(LargeItemCount)]
        public void AddTimed_TwoTimes_Concurrent(int itemCount)
        {
            var tasks = new List<Task>();
            for (var i = 0; i < itemCount; ++i)
            {
                var l = i;
                var task = Task.Factory.StartNew(() =>
                {
                    Cache.AddTimed(StringItems[l], StringItems[l], Cache.Clock.UtcNow.Subtract(TimeSpan.FromMinutes(10)));
                    Cache.Contains(StringItems[l]);
                });
                tasks.Add(task);
            }
            for (var i = 0; i < itemCount; ++i)
            {
                var l = i;
                var task = Task.Factory.StartNew(() =>
                {
                    Cache.AddTimed(StringItems[l], StringItems[l], Cache.Clock.UtcNow.Subtract(TimeSpan.FromMinutes(10)));
                    Cache.Contains(StringItems[l]);
                });
                tasks.Add(task);
            }
            foreach (var task in tasks)
            {
                task.Wait();
            }
        }

        [Test]
        public void AddTimed_TwoValues_SameKey()
        {
            Cache.AddTimed(StringItems[0], StringItems[1], Cache.Clock.UtcNow.AddMinutes(10));
            Assert.AreEqual(StringItems[1], Cache.Get<string>(StringItems[0]).Value);
            Cache.AddTimed(StringItems[0], StringItems[2], Cache.Clock.UtcNow.AddMinutes(10));
            Assert.AreEqual(StringItems[2], Cache.Get<string>(StringItems[0]).Value);
        }

        [Test]
        public void Count_EmptyCache()
        {
            Assert.AreEqual(0, Cache.Count());
            Assert.AreEqual(0, Cache.Count(StringItems[0]));
            Assert.AreEqual(0, Cache.LongCount());
            Assert.AreEqual(0, Cache.LongCount(StringItems[0]));
        }

        [TestCase(SmallItemCount)]
        [TestCase(MediumItemCount)]
        [TestCase(LargeItemCount)]
        public void Count_FullCache_OnePartition(int itemCount)
        {
            var partition = StringItems[0];
            for (var i = 0; i < itemCount; ++i)
            {
                Cache.AddStatic(partition, StringItems[i], StringItems[i]);
            }
            Assert.AreEqual(itemCount, Cache.Count());
            Assert.AreEqual(itemCount, Cache.Count(partition));
            Assert.AreEqual(itemCount, Cache.LongCount());
            Assert.AreEqual(itemCount, Cache.LongCount(partition));
        }

        [TestCase(SmallItemCount)]
        [TestCase(MediumItemCount)]
        [TestCase(LargeItemCount)]
        public void Count_FullCache_TwoPartitions(int itemCount)
        {
            int i;
            var partition1 = StringItems[0];
            var p1Count = itemCount / 3;
            for (i = 0; i < itemCount / 3; ++i)
            {
                Cache.AddStatic(partition1, StringItems[i], StringItems[i]);
            }
            var partition2 = StringItems[1];
            for (; i < itemCount; ++i)
            {
                Cache.AddStatic(partition2, StringItems[i], StringItems[i]);
            }
            Assert.AreEqual(itemCount, Cache.Count());
            Assert.AreEqual(p1Count, Cache.Count(partition1));
            Assert.AreEqual(itemCount - p1Count, Cache.Count(partition2));
            Assert.AreEqual(itemCount, Cache.LongCount());
            Assert.AreEqual(p1Count, Cache.LongCount(partition1));
            Assert.AreEqual(itemCount - p1Count, Cache.LongCount(partition2));
        }

        [TestCase(SmallItemCount)]
        [TestCase(MediumItemCount)]
        [TestCase(LargeItemCount)]
        public void Indexer_EmptyCache(int itemCount)
        {
            for (var i = 0; i < itemCount; ++i)
            {
                Assert.IsNull(Cache[Cache.Settings.DefaultPartition, StringItems[i]]);
                Assert.IsNull(Cache[StringItems[i]]);
            }
        }

        [TestCase(SmallItemCount)]
        [TestCase(MediumItemCount)]
        [TestCase(LargeItemCount)]
        public void Get_EmptyCache(int itemCount)
        {
            for (var i = 0; i < itemCount; ++i)
            {
                Assert.IsNull(Cache.Get<object>(Cache.Settings.DefaultPartition, StringItems[i]));
                Assert.IsNull(Cache.Get<object>(StringItems[i]));
            }
        }

        [TestCase(SmallItemCount)]
        [TestCase(MediumItemCount)]
        [TestCase(LargeItemCount)]
        public void Get_Typed_EmptyCache(int itemCount)
        {
            for (var i = 0; i < itemCount; ++i)
            {
                Assert.IsNull(Cache.Get<string>(Cache.Settings.DefaultPartition, StringItems[i]));
                Assert.IsNull(Cache.Get<string>(StringItems[i]));
            }
        }

        [TestCase(SmallItemCount)]
        [TestCase(MediumItemCount)]
        [TestCase(LargeItemCount)]
        public void GetItem_EmptyCache(int itemCount)
        {
            for (var i = 0; i < itemCount; ++i)
            {
                Assert.IsNull(Cache.GetItem<object>(Cache.Settings.DefaultPartition, StringItems[i]));
                Assert.IsNull(Cache.GetItem<object>(StringItems[i]));
            }
        }

        [TestCase(SmallItemCount)]
        [TestCase(MediumItemCount)]
        [TestCase(LargeItemCount)]
        public void GetItem_Typed_EmptyCache(int itemCount)
        {
            for (var i = 0; i < itemCount; ++i)
            {
                Assert.IsNull(Cache.GetItem<string>(Cache.Settings.DefaultPartition, StringItems[i]));
                Assert.IsNull(Cache.GetItem<string>(StringItems[i]));
            }
        }

        [TestCase(SmallItemCount)]
        [TestCase(MediumItemCount)]
        [TestCase(LargeItemCount)]
        public void Get_EmptyCache_Concurrent(int itemCount)
        {
            var tasks = new List<Task<FSharpOption<string>>>();
            for (var i = 0; i < itemCount; ++i)
            {
                var l = i;
                var task = Task.Run(() => Cache.Get<string>(StringItems[l]));
                tasks.Add(task);
            }
            for (var i = 0; i < itemCount; ++i)
            {
                Assert.IsNull(tasks[i].Result);
            }
        }

        [TestCase(SmallItemCount)]
        [TestCase(MediumItemCount)]
        [TestCase(LargeItemCount)]
        public void Get_FullCache(int itemCount)
        {
            for (var i = 0; i < itemCount; ++i)
            {
                Cache.AddTimed(StringItems[i], StringItems[i], Cache.Clock.UtcNow.AddMinutes(10));
            }
            for (var i = 0; i < itemCount; ++i)
            {
                var item = Cache.Get<string>(StringItems[i]);
                Assert.AreEqual(StringItems[i], item.Value);
            }
        }

        [TestCase(SmallItemCount)]
        [TestCase(MediumItemCount)]
        [TestCase(LargeItemCount)]
        public void Get_FullCache_Outdated(int itemCount)
        {
            for (var i = 0; i < itemCount; ++i)
            {
                Cache.AddTimed(StringItems[i], StringItems[i], Cache.Clock.UtcNow.Subtract(TimeSpan.FromMinutes(10)));
            }
            for (var i = 0; i < itemCount; ++i)
            {
                Assert.IsNull(Cache.Get<string>(StringItems[i]));
            }
        }

        [Test]
        public void Get_LargeDataTable()
        {
            var dt = new RandomDataTableGenerator("A123", "Test", "Pi", "<3", "Pu").GenerateDataTable(LargeItemCount);
            Cache.AddStatic(dt.TableName, dt);
            var storedDt = Cache.Get<DataTable>(dt.TableName).Value;
            Assert.AreEqual(dt.Rows.Count, storedDt.Rows.Count);
            for (var i = 0; i < dt.Rows.Count; ++i)
            {
                Assert.AreEqual(dt.Rows[i].ItemArray.Length, storedDt.Rows[i].ItemArray.Length);
                for (var j = 0; j < dt.Rows[i].ItemArray.Length; ++j)
                {
                    Assert.AreEqual(dt.Rows[i].ItemArray[j], storedDt.Rows[i].ItemArray[j]);
                }
            }
        }

        [TestCase(SmallItemCount)]
        [TestCase(MediumItemCount)]
        [TestCase(LargeItemCount)]
        public void GetMany_RightItems_AfterAddSliding_InvalidTime(int itemCount)
        {
            AddSliding(Cache, itemCount, TimeSpan.FromSeconds(1));
            Cache.Clock.As<MockClock>().Add(TimeSpan.FromSeconds(2));
            var items = new HashSet<string>(Cache.GetItems<string>().Select(i => i.Value));
            for (var i = 0; i < itemCount; ++i)
            {
                Assert.False(items.Contains(StringItems[i]));
            }
            items = new HashSet<string>(Cache.GetItems<string>().Select(x => x.Value));
            for (var i = 0; i < itemCount; ++i)
            {
                Assert.False(items.Contains(StringItems[i]));
            }
        }

        [TestCase(SmallItemCount)]
        [TestCase(MediumItemCount)]
        [TestCase(LargeItemCount)]
        public void GetMany_RightItems_AfterAddSliding_ValidTime(int itemCount)
        {
            AddSliding(Cache, itemCount, TimeSpan.FromHours(1));
            var items = new HashSet<string>(Cache.GetItems<string>().Select(i => i.Value));
            for (var i = 0; i < itemCount; ++i)
            {
                Assert.True(items.Contains(StringItems[i]));
            }
            items = new HashSet<string>(Cache.GetItems<string>().Select(x => x.Value));
            for (var i = 0; i < itemCount; ++i)
            {
                Assert.True(items.Contains(StringItems[i]));
            }
        }

        [TestCase(SmallItemCount)]
        [TestCase(MediumItemCount)]
        [TestCase(LargeItemCount)]
        public void GetMany_RightItems_AfterAddStatic(int itemCount)
        {
            AddStatic(Cache, itemCount);
            var items = new HashSet<string>(Cache.GetItems<string>().Select(i => i.Value));
            for (var i = 0; i < itemCount; ++i)
            {
                Assert.True(items.Contains(StringItems[i]));
            }
            items = new HashSet<string>(Cache.GetItems<string>().Select(x => x.Value));
            for (var i = 0; i < itemCount; ++i)
            {
                Assert.True(items.Contains(StringItems[i]));
            }
        }

        [TestCase(SmallItemCount)]
        [TestCase(MediumItemCount)]
        [TestCase(LargeItemCount)]
        public void GetMany_RightItems_AfterAddTimed_InvalidTime(int itemCount)
        {
            AddTimed(Cache, itemCount, Cache.Clock.UtcNow.Subtract(TimeSpan.FromMinutes(10)));
            var items = new HashSet<string>(Cache.GetItems<string>().Select(i => i.Value));
            for (var i = 0; i < itemCount; ++i)
            {
                Assert.False(items.Contains(StringItems[i]));
            }
            items = new HashSet<string>(Cache.GetItems<string>().Select(x => x.Value));
            for (var i = 0; i < itemCount; ++i)
            {
                Assert.False(items.Contains(StringItems[i]));
            }
        }

        [TestCase(SmallItemCount)]
        [TestCase(MediumItemCount)]
        [TestCase(LargeItemCount)]
        public void GetMany_RightItems_AfterAddTimed_ValidTime(int itemCount)
        {
            AddTimed(Cache, itemCount, Cache.Clock.UtcNow.AddMinutes(10));
            var items = new HashSet<string>(Cache.GetItems<string>().Select(i => i.Value));
            for (var i = 0; i < itemCount; ++i)
            {
                Assert.True(items.Contains(StringItems[i]));
            }
            items = new HashSet<string>(Cache.GetItems<string>().Select(x => x.Value));
            for (var i = 0; i < itemCount; ++i)
            {
                Assert.True(items.Contains(StringItems[i]));
            }
        }

        [TestCase(SmallItemCount)]
        [TestCase(MediumItemCount)]
        [TestCase(LargeItemCount)]
        public void GetItem_FullSlidingCache_TimeIncreased(int itemCount)
        {
            var interval = TimeSpan.FromMinutes(10);
            for (var i = 0; i < itemCount; ++i)
            {
                Cache.AddSliding(StringItems[i], StringItems[i], interval);
            }
            for (var i = 0; i < itemCount; ++i)
            {
                var item = Cache.GetItem<string>(StringItems[i]).Value;
                Assert.IsNotNull(item);
                Assert.AreEqual(item.UtcExpiry.ToUnixTime(), (Cache.Clock.UtcNow + interval).ToUnixTime());
            }
        }

        [TestCase(SmallItemCount)]
        [TestCase(MediumItemCount)]
        [TestCase(LargeItemCount)]
        public void GetManyItems_FullSlidingCache_TimeIncreased(int itemCount)
        {
            var interval = TimeSpan.FromMinutes(10);
            for (var i = 0; i < itemCount; ++i)
            {
                Cache.AddSliding(StringItems[i], StringItems[i], interval);
            }
            Cache.Clock.As<MockClock>().Add(TimeSpan.FromMinutes(1));
            var items = Cache.GetItems<string>();
            for (var i = 0; i < itemCount; ++i)
            {
                var item = items[i];
                Assert.IsNotNull(item);
                Assert.AreEqual(item.UtcExpiry.ToUnixTime(), (Cache.Clock.UtcNow + interval).ToUnixTime());
            }
        }

        [Test]
        public void Clean_AfterFixedNumberOfInserts_InvalidValues()
        {
            for (var i = 0; i < Cache.Settings.InsertionCountBeforeAutoClean; ++i)
            {
                Cache.AddTimed(StringItems[i], StringItems[i], Cache.Clock.UtcNow.Subtract(TimeSpan.FromMinutes(10)));
            }
            Assert.AreEqual(0, Cache.Count());
        }

        [Test]
        public void Clean_AfterFixedNumberOfInserts_ValidValues()
        {
            for (var i = 0; i < Cache.Settings.InsertionCountBeforeAutoClean; ++i)
            {
                Cache.AddTimed(StringItems[i], StringItems[i], Cache.Clock.UtcNow.AddMinutes(10));
            }
            Assert.AreEqual(Cache.Settings.InsertionCountBeforeAutoClean, Cache.Count());
        }

        [TestCase(SmallItemCount)]
        [TestCase(MediumItemCount)]
        [TestCase(LargeItemCount)]
        public void Peek_EmptyCache(int itemCount)
        {
            for (var i = 0; i < itemCount; ++i)
            {
                Assert.IsNull(Cache.Peek<object>(Cache.Settings.DefaultPartition, StringItems[i]));
                Assert.IsNull(Cache.Peek<object>(StringItems[i]));
            }
        }

        [TestCase(SmallItemCount)]
        [TestCase(MediumItemCount)]
        [TestCase(LargeItemCount)]
        public void Peek_Typed_EmptyCache(int itemCount)
        {
            for (var i = 0; i < itemCount; ++i)
            {
                Assert.IsNull(Cache.Peek<string>(Cache.Settings.DefaultPartition, StringItems[i]));
                Assert.IsNull(Cache.Peek<string>(StringItems[i]));
            }
        }

        [TestCase(SmallItemCount)]
        [TestCase(MediumItemCount)]
        [TestCase(LargeItemCount)]
        public void PeekItem_EmptyCache(int itemCount)
        {
            for (var i = 0; i < itemCount; ++i)
            {
                Assert.IsNull(Cache.PeekItem<object>(Cache.Settings.DefaultPartition, StringItems[i]));
                Assert.IsNull(Cache.PeekItem<object>(StringItems[i]));
            }
        }

        [TestCase(SmallItemCount)]
        [TestCase(MediumItemCount)]
        [TestCase(LargeItemCount)]
        public void PeekItem_Typed_EmptyCache(int itemCount)
        {
            for (var i = 0; i < itemCount; ++i)
            {
                Assert.IsNull(Cache.PeekItem<string>(Cache.Settings.DefaultPartition, StringItems[i]));
                Assert.IsNull(Cache.PeekItem<string>(StringItems[i]));
            }
        }

        [TestCase(SmallItemCount)]
        [TestCase(MediumItemCount)]
        [TestCase(LargeItemCount)]
        public void Peek_EmptyCache_Concurrent(int itemCount)
        {
            var tasks = new List<Task<FSharpOption<string>>>();
            for (var i = 0; i < itemCount; ++i)
            {
                var l = i;
                var task = Task.Run(() => Cache.Peek<string>(StringItems[l]));
                tasks.Add(task);
            }
            for (var i = 0; i < itemCount; ++i)
            {
                Assert.IsNull(tasks[i].Result);
            }
        }

        [TestCase(SmallItemCount)]
        [TestCase(MediumItemCount)]
        [TestCase(LargeItemCount)]
        public void Peek_FullCache_ExpiryNotChanged(int itemCount)
        {
            var expiryDate = Cache.Clock.UtcNow.AddMinutes(10);
            for (var i = 0; i < itemCount; ++i)
            {
                Cache.AddTimed(StringItems[i], StringItems[i], expiryDate);
            }
            for (var i = 0; i < itemCount; ++i)
            {
                var value = Cache.Peek<string>(StringItems[i]);
                Assert.AreEqual(StringItems[i], value.Value);
                var item = Cache.PeekItem<string>(StringItems[i]).Value;
                Assert.AreEqual(expiryDate.Date, item.UtcExpiry.Date);
                Assert.AreEqual(expiryDate.Hour, item.UtcExpiry.Hour);
                Assert.AreEqual(expiryDate.Minute, item.UtcExpiry.Minute);
                Assert.AreEqual(expiryDate.Second, item.UtcExpiry.Second);
            }
        }

        [TestCase(SmallItemCount)]
        [TestCase(MediumItemCount)]
        [TestCase(LargeItemCount)]
        public void Peek_FullCache_Outdated(int itemCount)
        {
            for (var i = 0; i < itemCount; ++i)
            {
                Cache.AddTimed(StringItems[i], StringItems[i], Cache.Clock.UtcNow.Subtract(TimeSpan.FromMinutes(10)));
            }
            for (var i = 0; i < itemCount; ++i)
            {
                Assert.IsNull(Cache.Peek<string>(StringItems[i]));
            }
        }

        #region Serialization

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void AddStatic_NotSerializableValue()
        {
            Cache.AddStatic(StringItems[0], new NotSerializableClass());
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void AddStatic_DataContractValue()
        {
            Cache.AddStatic(StringItems[0], new DataContractClass());
        }

        #endregion Serialization

        #region BCL Collections

        [TestCase(SmallItemCount)]
        [TestCase(MediumItemCount)]
        [TestCase(LargeItemCount)]
        public void Collections_Dictionary(int itemCount)
        {
            var l = Enumerable.Range(1, itemCount).ToDictionary(i => i, i => i.ToString(CultureInfo.InvariantCulture));
            Cache.AddStatic("dict", l);
            var lc = Cache.Get<Dictionary<int, string>>("dict").Value;
            Assert.True(l.SequenceEqual(lc));
        }

        [TestCase(SmallItemCount)]
        [TestCase(MediumItemCount)]
        [TestCase(LargeItemCount)]
        public void Collections_List(int itemCount)
        {
            var l = new List<int>(Enumerable.Range(1, itemCount));
            Cache.AddStatic("list", l);
            var lc = Cache.Get<List<int>>("list").Value;
            Assert.True(l.SequenceEqual(lc));
        }

        [TestCase(SmallItemCount)]
        [TestCase(MediumItemCount)]
        [TestCase(LargeItemCount)]
        public void Collections_HashSet(int itemCount)
        {
            var l = new HashSet<int>(Enumerable.Range(1, itemCount));
            Cache.AddStatic("set", l);
            var lc = Cache.Get<HashSet<int>>("set").Value;
            Assert.True(l.SequenceEqual(lc));
        }

        [TestCase(SmallItemCount)]
        [TestCase(MediumItemCount)]
        [TestCase(LargeItemCount)]
        public void Collections_SortedSet(int itemCount)
        {
            var l = new SortedSet<int>(Enumerable.Range(1, itemCount));
            Cache.AddStatic("set", l);
            var lc = Cache.Get<SortedSet<int>>("set").Value;
            Assert.True(l.SequenceEqual(lc));
        }

        [TestCase(SmallItemCount)]
        [TestCase(MediumItemCount)]
        [TestCase(LargeItemCount)]
        public void Collections_SortedList(int itemCount)
        {
            var l = new SortedList<int, string>(Enumerable.Range(1, itemCount).ToDictionary(i => i, i => i.ToString(CultureInfo.InvariantCulture)));
            Cache.AddStatic("list", l);
            var lc = Cache.Get<SortedList<int, string>>("list").Value;
            Assert.True(l.SequenceEqual(lc));
        }

        [TestCase(SmallItemCount)]
        [TestCase(MediumItemCount)]
        [TestCase(LargeItemCount)]
        public void Collections_FSharpList(int itemCount)
        {
            var fl = FSharpList.Create(Enumerable.Range(1, itemCount).ToArray());
            Cache.AddStatic("f#_list", fl);
            var lc = Cache.Get<FSharpList<int>>("f#_list").Value;
            Assert.True(fl.SequenceEqual(lc));
        }

        #endregion BCL Collections

        #region Private Methods

        private void AddSliding(ICache instance, int itemCount, TimeSpan interval)
        {
            for (var i = 0; i < itemCount; ++i)
            {
                instance.AddSliding(StringItems[i], StringItems[i], interval);
            }
        }

        private void AddStatic(ICache instance, int itemCount)
        {
            for (var i = 0; i < itemCount; ++i)
            {
                instance.AddStatic(StringItems[i], StringItems[i]);
            }
        }

        private void AddTimed(ICache instance, int itemCount, DateTime utcTime)
        {
            for (var i = 0; i < itemCount; ++i)
            {
                instance.AddTimed(StringItems[i], StringItems[i], utcTime);
            }
        }

        #endregion Private Methods
    }

    internal sealed class NotSerializableClass
    {
        public string Pino = "Gino";
    }

    [DataContract]
    internal sealed class DataContractClass
    {
        [DataMember]
        public string Pino = "Gino";
    }
}
