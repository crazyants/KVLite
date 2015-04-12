﻿// File name: CacheExtensions.cs
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
using System.Diagnostics.Contracts;
using System.Threading.Tasks;
using PommaLabs.KVLite.Core;

namespace PommaLabs.KVLite
{
    /// <summary>
    ///   Extension methods for the <see cref="ICache"/> interface.
    /// </summary>
    public static class CacheExtensions
    {
        #region Extensions - Default Partition

        /// <summary>
        ///   Adds a "sliding" value with given key. Value will last as much as specified in given
        ///   interval and, if accessed before expiry, its lifetime will be extended by the interval itself.
        /// </summary>
        /// <param name="cache">The cache.</param>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <param name="interval">The interval.</param>
        public static void AddSliding(this ICache cache, string key, object value, TimeSpan interval)
        {
            cache.AddSliding(cache.Settings.DefaultPartition, key, value, interval);
        }

        /// <summary>
        ///   Adds a "static" value with given partition and key. Value will last as much as
        ///   specified in <see cref="CacheSettingsBase.StaticIntervalInDays"/> and, if accessed
        ///   before expiry, its lifetime will be extended by that interval.
        /// </summary>
        /// <param name="cache">The cache.</param>
        /// <param name="partition">The partition.</param>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        public static void AddStatic(this ICache cache, string partition, string key, object value)
        {
            cache.AddSliding(partition, key, value, cache.Settings.StaticInterval);
        }

        /// <summary>
        ///   Adds a "static" value with given key. Value will last as much as specified in
        ///   <see cref="CacheSettingsBase.StaticIntervalInDays"/> and, if accessed before expiry,
        ///   its lifetime will be extended by that interval.
        /// </summary>
        /// <param name="cache">The cache.</param>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        public static void AddStatic(this ICache cache, string key, object value)
        {
            cache.AddSliding(cache.Settings.DefaultPartition, key, value, cache.Settings.StaticInterval);
        }

        /// <summary>
        ///   Adds a "timed" value with given key. Value will last until the specified time and, if
        ///   accessed before expiry, its lifetime will _not_ be extended.
        /// </summary>
        /// <param name="cache">The cache.</param>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <param name="utcExpiry">The UTC expiry.</param>
        public static void AddTimed(this ICache cache, string key, object value, DateTime utcExpiry)
        {
            cache.AddTimed(cache.Settings.DefaultPartition, key, value, utcExpiry);
        }

        /// <summary>
        ///   Determines whether cache the specified key.
        /// </summary>
        /// <param name="cache">The cache.</param>
        /// <param name="key">The key.</param>
        /// <returns>True if key is contained, false otherwise.</returns>
        [Pure]
        public static bool Contains(this ICache cache, string key)
        {
            return cache.Contains(cache.Settings.DefaultPartition, key);
        }

        /// <summary>
        ///   The number of elements inside the cache.
        /// </summary>
        /// <param name="cache">The cache.</param>
        /// <returns>The number of elements inside the cache.</returns>
        [Pure]
        public static int Count(this ICache cache)
        {
            Contract.Ensures(Contract.Result<int>() >= 0);
            return Convert.ToInt32(cache.LongCount());
        }

        /// <summary>
        ///   The number of elements inside given partition.
        /// </summary>
        /// <param name="cache">The cache.</param>
        /// <param name="partition">The partition.</param>
        /// <returns>The number of elements inside given partition.</returns>
        [Pure]
        public static int Count(this ICache cache, string partition)
        {
            Contract.Ensures(Contract.Result<int>() >= 0);
            return Convert.ToInt32(cache.LongCount(partition));
        }

        /// <summary>
        ///   Gets the value with specified partition and key. If it is a "sliding" or "static"
        ///   value, its lifetime will be increased by corresponding interval.
        /// </summary>
        /// <param name="cache">The cache.</param>
        /// <param name="key">The key.</param>
        /// <returns>The value with specified partition and key.</returns>
        public static object Get(this ICache cache, string key)
        {
            return cache.Get(cache.Settings.DefaultPartition, key);
        }

        /// <summary>
        ///   Gets the cache item with specified key. If it is a "sliding" or "static" value, its
        ///   lifetime will be increased by corresponding interval.
        /// </summary>
        /// <param name="cache">The cache.</param>
        /// <param name="key">The key.</param>
        /// <returns>The cache item with specified key.</returns>
        public static CacheItem GetItem(this ICache cache, string key)
        {
            return cache.GetItem(cache.Settings.DefaultPartition, key);
        }

        /// <summary>
        ///   Gets the value corresponding to given key, without updating expiry date.
        /// </summary>
        /// <returns>The value corresponding to given key, without updating expiry date.</returns>
        [Pure]
        public static object Peek(this ICache cache, string key)
        {
            return cache.Peek(cache.Settings.DefaultPartition, key);
        }

        /// <summary>
        ///   Gets the item corresponding to given key, without updating expiry date.
        /// </summary>
        /// <returns>The item corresponding to given key, without updating expiry date.</returns>
        [Pure]
        public static CacheItem PeekItem(this ICache cache, string key)
        {
            return cache.PeekItem(cache.Settings.DefaultPartition, key);
        }

        /// <summary>
        ///   Removes the specified key.
        /// </summary>
        /// <param name="cache">The cache.</param>
        /// <param name="key">The key.</param>
        public static void Remove(this ICache cache, string key)
        {
            cache.Remove(cache.Settings.DefaultPartition, key);
        }

        #endregion Extensions - Default Partition

        #region Extensions - Async Methods

        /// <summary>
        ///   Adds a "sliding" value with given partition and key. Value will last as much as
        ///   specified in given interval and, if accessed before expiry, its lifetime will be
        ///   extended by the interval itself.
        /// </summary>
        /// <param name="cache">The cache.</param>
        /// <param name="partition">The partition.</param>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <param name="interval">The interval.</param>
        /// <returns>The insertion task.</returns>
        public static Task AddSlidingAsync(this ICache cache, string partition, string key, object value, TimeSpan interval)
        {
            Contract.Ensures(Contract.Result<Task>() != null);
            Contract.Ensures(Contract.Result<Task>().Status != TaskStatus.Created);
            return TaskRunner.Run(() => cache.AddSliding(partition, key, value, interval));
        }

        /// <summary>
        ///   Adds a "sliding" value with given key. Value will last as much as specified in given
        ///   interval and, if accessed before expiry, its lifetime will be extended by the interval itself.
        /// </summary>
        /// <param name="cache">The cache.</param>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <param name="interval">The interval.</param>
        /// <returns>The insertion task.</returns>
        public static Task AddSlidingAsync(this ICache cache, string key, object value, TimeSpan interval)
        {
            Contract.Ensures(Contract.Result<Task>() != null);
            Contract.Ensures(Contract.Result<Task>().Status != TaskStatus.Created);
            return TaskRunner.Run(() => cache.AddSliding(cache.Settings.DefaultPartition, key, value, interval));
        }

        /// <summary>
        ///   Adds a "static" value with given partition and key. Value will last as much as
        ///   specified in <see cref="CacheSettingsBase.StaticIntervalInDays"/> and, if accessed
        ///   before expiry, its lifetime will be extended by that interval.
        /// </summary>
        /// <param name="cache">The cache.</param>
        /// <param name="partition">The partition.</param>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <returns>The insertion task.</returns>
        public static Task AddStaticAsync(this ICache cache, string partition, string key, object value)
        {
            Contract.Ensures(Contract.Result<Task>() != null);
            Contract.Ensures(Contract.Result<Task>().Status != TaskStatus.Created);
            return TaskRunner.Run(() => cache.AddSliding(partition, key, value, cache.Settings.StaticInterval));
        }

        /// <summary>
        ///   Adds a "static" value with given key. Value will last as much as specified in
        ///   <see cref="CacheSettingsBase.StaticIntervalInDays"/> and, if accessed before expiry,
        ///   its lifetime will be extended by that interval.
        /// </summary>
        /// <param name="cache">The cache.</param>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <returns>The insertion task.</returns>
        public static Task AddStaticAsync(this ICache cache, string key, object value)
        {
            Contract.Ensures(Contract.Result<Task>() != null);
            Contract.Ensures(Contract.Result<Task>().Status != TaskStatus.Created);
            return TaskRunner.Run(() => cache.AddSliding(cache.Settings.DefaultPartition, key, value, cache.Settings.StaticInterval));
        }

        /// <summary>
        ///   Adds a "timed" value with given partition and key. Value will last until the specified
        ///   time and, if accessed before expiry, its lifetime will _not_ be extended.
        /// </summary>
        /// <param name="cache">The cache.</param>
        /// <param name="partition">The partition.</param>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <param name="utcExpiry">The UTC expiry.</param>
        /// <returns>The insertion task.</returns>
        public static Task AddTimedAsync(this ICache cache, string partition, string key, object value, DateTime utcExpiry)
        {
            Contract.Ensures(Contract.Result<Task>() != null);
            Contract.Ensures(Contract.Result<Task>().Status != TaskStatus.Created);
            return TaskRunner.Run(() => cache.AddTimed(partition, key, value, utcExpiry));
        }

        /// <summary>
        ///   Adds a "timed" value with given key. Value will last until the specified time and, if
        ///   accessed before expiry, its lifetime will _not_ be extended.
        /// </summary>
        /// <param name="cache">The cache.</param>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <param name="utcExpiry">The UTC expiry.</param>
        /// <returns>The insertion task.</returns>
        public static Task AddTimedAsync(this ICache cache, string key, object value, DateTime utcExpiry)
        {
            Contract.Ensures(Contract.Result<Task>() != null);
            Contract.Ensures(Contract.Result<Task>().Status != TaskStatus.Created);
            return TaskRunner.Run(() => cache.AddTimed(cache.Settings.DefaultPartition, key, value, utcExpiry));
        }

        /// <summary>
        ///   Removes the value with given partition and key.
        /// </summary>
        /// <param name="cache">The cache.</param>
        /// <param name="partition">The partition.</param>
        /// <param name="key">The key.</param>
        /// <returns>The removal task.</returns>
        public static Task RemoveAsync(this ICache cache, string partition, string key)
        {
            Contract.Ensures(Contract.Result<Task>() != null);
            Contract.Ensures(Contract.Result<Task>().Status != TaskStatus.Created);
            return TaskRunner.Run(() => cache.Remove(partition, key));
        }

        /// <summary>
        ///   Removes the specified key.
        /// </summary>
        /// <param name="cache">The cache.</param>
        /// <param name="key">The key.</param>
        /// <returns>The removal task.</returns>
        public static Task RemoveAsync(this ICache cache, string key)
        {
            Contract.Ensures(Contract.Result<Task>() != null);
            Contract.Ensures(Contract.Result<Task>().Status != TaskStatus.Created);
            return TaskRunner.Run(() => cache.Remove(cache.Settings.DefaultPartition, key));
        }

        #endregion Extensions - Async Methods

        #region Extensions - Typed Retrieval

        public static TVal Get<TVal>(this ICache cache, string partition, string key)
        {
            return (TVal) cache.Get(partition, key);
        }

        public static TVal Get<TVal>(this ICache cache, string key)
        {
            return (TVal) cache.Get(cache.Settings.DefaultPartition, key);
        }

        public static CacheItem<TVal> GetItem<TVal>(this ICache cache, string partition, string key)
        {
            var item = cache.GetItem(partition, key);
            return (item == null)
                ? null
                : new CacheItem<TVal>
                {
                    Partition = item.Partition,
                    Key = item.Key,
                    Value = (TVal) item.Value,
                    UtcCreation = item.UtcCreation,
                    UtcExpiry = item.UtcExpiry,
                    Interval = item.Interval
                };
        }

        public static CacheItem<TVal> GetItem<TVal>(this ICache cache, string key)
        {
            var item = cache.GetItem(cache.Settings.DefaultPartition, key);
            return (item == null)
                ? null
                : new CacheItem<TVal>
                {
                    Partition = item.Partition,
                    Key = item.Key,
                    Value = (TVal) item.Value,
                    UtcCreation = item.UtcCreation,
                    UtcExpiry = item.UtcExpiry,
                    Interval = item.Interval
                };
        }

        public static TVal Peek<TVal>(this ICache cache, string partition, string key)
        {
            return (TVal) cache.Peek(partition, key);
        }

        public static TVal Peek<TVal>(this ICache cache, string key)
        {
            return (TVal) cache.Peek(cache.Settings.DefaultPartition, key);
        }

        public static CacheItem<TVal> PeekItem<TVal>(this ICache cache, string partition, string key)
        {
            var item = cache.PeekItem(partition, key);
            return (item == null)
                ? null
                : new CacheItem<TVal>
                {
                    Partition = item.Partition,
                    Key = item.Key,
                    Value = (TVal) item.Value,
                    UtcCreation = item.UtcCreation,
                    UtcExpiry = item.UtcExpiry,
                    Interval = item.Interval
                };
        }

        public static CacheItem<TVal> PeekItem<TVal>(this ICache cache, string key)
        {
            var item = cache.PeekItem(cache.Settings.DefaultPartition, key);
            return (item == null)
                ? null
                : new CacheItem<TVal>
                {
                    Partition = item.Partition,
                    Key = item.Key,
                    Value = (TVal) item.Value,
                    UtcCreation = item.UtcCreation,
                    UtcExpiry = item.UtcExpiry,
                    Interval = item.Interval
                };
        }

        #endregion Extensions - Typed Retrieval
    }
}