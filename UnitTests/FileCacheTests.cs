﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using KVLite;
using NUnit.Framework;

namespace UnitTests
{
    [TestFixture]
    public sealed class PersistentCacheTests
    {
        [SetUp]
        public void SetUp()
        {
            _fileCache = new PersistentCache();
            _fileCache.Clear(true);
        }

        [TearDown]
        public void TearDown()
        {
            _fileCache.Clear(true);
            _fileCache = null;
        }

        private const int SmallItemCount = 10;
        private const int MediumItemCount = 100;
        private const int LargeItemCount = 1000;
        private const int MinItem = 10000;
        private const string BlankPath = "   ";

        private static readonly List<string> StringItems = (from x in Enumerable.Range(MinItem, LargeItemCount)
            select x.ToString(CultureInfo.InvariantCulture)).ToList();

        private PersistentCache _fileCache;

        [TestCase(SmallItemCount)]
        [TestCase(MediumItemCount)]
        [TestCase(LargeItemCount)]
        public void Add_TwoTimes(int itemCount)
        {
            for (var i = 0; i < itemCount; ++i)
            {
                _fileCache.Add(StringItems[i], StringItems[i], DateTime.UtcNow.Add(TimeSpan.FromMinutes(10)));
                Assert.True(_fileCache.Contains(StringItems[i]));
            }
            for (var i = 0; i < itemCount; ++i)
            {
                _fileCache.Add(StringItems[i], StringItems[i], DateTime.UtcNow.Add(TimeSpan.FromMinutes(10)));
                Assert.True(_fileCache.Contains(StringItems[i]));
            }
        }

        [TestCase(SmallItemCount)]
        [TestCase(MediumItemCount)]
        public void Add_TwoTimes_Concurrent(int itemCount)
        {
            for (var i = 0; i < itemCount; ++i)
            {
                var l = i;
                Task.Factory.StartNew(() =>
                {
                    _fileCache.Add(StringItems[l], StringItems[l], DateTime.UtcNow.Subtract(TimeSpan.FromMinutes(10)));
                    _fileCache.Contains(StringItems[l]);
                });
            }
            for (var i = 0; i < itemCount; ++i)
            {
                var l = i;
                Task.Factory.StartNew(() =>
                {
                    _fileCache.Add(StringItems[l], StringItems[l], DateTime.UtcNow.Subtract(TimeSpan.FromMinutes(10)));
                    _fileCache.Contains(StringItems[l]);
                });
            }
        }

        [Test]
        public void AddPersistent_RightInfo()
        {
            var p = StringItems[0];
            var k = StringItems[1];
            var v1 = StringItems[2];
            var v2 = StringItems[3];
            _fileCache.AddPersistent(p, k, Tuple.Create(v1, v2));
            var info = _fileCache.GetInfo(p, k);
            Assert.IsNotNull(info);
            Assert.AreEqual(p, info.Partition);
            Assert.AreEqual(k, info.Key);
            var infoValue = info.Value as Tuple<string, string>;
            Assert.AreEqual(v1, infoValue.Item1);
            Assert.AreEqual(v2, infoValue.Item2);
            Assert.IsNull(info.Expiry);
            Assert.IsNull(info.Interval);
        }

        [Test]
        public void AddTimed_RightInfo()
        {
            var p = StringItems[0];
            var k = StringItems[1];
            var v1 = StringItems[2];
            var v2 = StringItems[3];
            var e = DateTime.Now.AddMinutes(10);
            _fileCache.AddTimed(p, k, Tuple.Create(v1, v2), e);
            var info = _fileCache.GetInfo(p, k);
            Assert.IsNotNull(info);
            Assert.AreEqual(p, info.Partition);
            Assert.AreEqual(k, info.Key);
            var infoValue = info.Value as Tuple<string, string>;
            Assert.AreEqual(v1, infoValue.Item1);
            Assert.AreEqual(v2, infoValue.Item2);

            Assert.IsNotNull(info.Expiry);
            Assert.AreEqual(e.Date, info.Expiry.Value.Date);
            Assert.AreEqual(e.Hour, info.Expiry.Value.Hour);
            Assert.AreEqual(e.Minute, info.Expiry.Value.Minute);
            Assert.AreEqual(e.Second, info.Expiry.Value.Second);
            
            Assert.IsNull(info.Interval);
        }

        [TestCase(SmallItemCount)]
        [TestCase(MediumItemCount)]
        [TestCase(LargeItemCount)]
        public void Get_EmptyCache(int itemCount)
        {
            for (var i = 0; i < itemCount; ++i)
            {
                Assert.IsNull(_fileCache.Get(StringItems[i]));
            }
        }

        [TestCase(SmallItemCount)]
        [TestCase(MediumItemCount)]
        public void Get_EmptyCache_Concurrent(int itemCount)
        {
            var tasks = new List<Task<object>>();
            for (var i = 0; i < itemCount; ++i)
            {
                var l = i;
                var task = Task.Factory.StartNew(() => _fileCache.Get(StringItems[l]));
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
                Assert.IsNotNull(_fileCache.Add(StringItems[i], StringItems[i], DateTime.UtcNow.AddMinutes(10)));
            }
            for (var i = 0; i < itemCount; ++i)
            {
                var item = (string) _fileCache.Get(StringItems[i]);
                Assert.IsNotNull(item);
                Assert.AreEqual(StringItems[i], item);
            }
        }

        [TestCase(SmallItemCount)]
        [TestCase(MediumItemCount)]
        [TestCase(LargeItemCount)]
        public void Get_FullCache_Outdated(int itemCount)
        {
            for (var i = 0; i < itemCount; ++i)
            {
                Assert.IsNotNull(_fileCache.Add(StringItems[i], StringItems[i], DateTime.UtcNow.Subtract(TimeSpan.FromMinutes(10))));
            }
            for (var i = 0; i < itemCount; ++i)
            {
                Assert.IsNull(_fileCache.Get(StringItems[i]));
            }
        }

        [Test]
        public void NewCache_BlankPath()
        {
            try
            {
                new PersistentCache(BlankPath);
            }
            catch (Exception ex)
            {
                Assert.IsInstanceOf<ArgumentException>(ex);
                Assert.AreEqual(ErrorMessages.NullOrEmptyCachePath, ex.Message);
            }
        }

        [Test]
        public void NewCache_EmptyPath()
        {
            try
            {
                new PersistentCache(String.Empty);
            }
            catch (Exception ex)
            {
                Assert.IsInstanceOf<ArgumentException>(ex);
                Assert.AreEqual(ErrorMessages.NullOrEmptyCachePath, ex.Message);
            }
        }

        [Test]
        public void NewCache_NullPath()
        {
            try
            {
                new PersistentCache(null);
            }
            catch (Exception ex)
            {
                Assert.IsInstanceOf<ArgumentException>(ex);
                Assert.AreEqual(ErrorMessages.NullOrEmptyCachePath, ex.Message);
            }
        }
    }
}