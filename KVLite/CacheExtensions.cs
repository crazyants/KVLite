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

namespace PommaLabs.KVLite
{
    public static class CacheExtensions
    {
        #region Default Partition

        public static void AddSliding(this ICache cache, string key, object value, TimeSpan interval)
        {
            cache.AddSliding(cache.Settings.DefaultPartition, key, value, interval);
        }

        public static void AddStatic(this ICache cache, string partition, string key, object value)
        {
            cache.AddSliding(partition, key, value, cache.Settings.StaticInterval);
        }

        public static void AddStatic(this ICache cache, string key, object value)
        {
            cache.AddSliding(cache.Settings.DefaultPartition, key, value, cache.Settings.StaticInterval);
        }

        public static void AddTimed(this ICache cache, string key, object value, DateTime utcExpiry)
        {
            cache.AddTimed(cache.Settings.DefaultPartition, key, value, utcExpiry);
        }

        [Pure]
        public static bool Contains(this ICache cache, string key)
        {
            return cache.Contains(cache.Settings.DefaultPartition, key);
        }

        [Pure]
        public static int Count(this ICache cache)
        {
            Contract.Ensures(Contract.Result<int>() >= 0);
            return Convert.ToInt32(cache.LongCount());
        }

        [Pure]
        public static int Count(this ICache cache, string partition)
        {
            Contract.Ensures(Contract.Result<int>() >= 0);
            return Convert.ToInt32(cache.LongCount(partition));
        }

        [Pure]
        public static object Get(this ICache cache, string key)
        {
            return cache.Get(cache.Settings.DefaultPartition, key);
        }

        [Pure]
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

        public static void Remove(this ICache cache, string key)
        {
            cache.Remove(cache.Settings.DefaultPartition, key);
        }

        #endregion

        #region Async Methods

        public static Task AddSlidingAsync(this ICache cache, string partition, string key, object value, TimeSpan interval)
        {
            Contract.Ensures(Contract.Result<Task>() != null);
            Contract.Ensures(Contract.Result<Task>().Status != TaskStatus.Created);
            return TaskEx.Run(() => cache.AddSliding(partition, key, value, interval));
        }

        public static Task AddSlidingAsync(this ICache cache, string key, object value, TimeSpan interval)
        {
            Contract.Ensures(Contract.Result<Task>() != null);
            Contract.Ensures(Contract.Result<Task>().Status != TaskStatus.Created);
            return TaskEx.Run(() => cache.AddSliding(cache.Settings.DefaultPartition, key, value, interval));
        }

        public static Task AddStaticAsync(this ICache cache, string partition, string key, object value)
        {
            Contract.Ensures(Contract.Result<Task>() != null);
            Contract.Ensures(Contract.Result<Task>().Status != TaskStatus.Created);
            return TaskEx.Run(() => cache.AddSliding(partition, key, value, cache.Settings.StaticInterval));
        }

        public static Task AddStaticAsync(this ICache cache, string key, object value)
        {
            Contract.Ensures(Contract.Result<Task>() != null);
            Contract.Ensures(Contract.Result<Task>().Status != TaskStatus.Created);
            return TaskEx.Run(() => cache.AddSliding(cache.Settings.DefaultPartition, key, value, cache.Settings.StaticInterval));
        }

        public static Task AddTimedAsync(this ICache cache, string partition, string key, object value, DateTime utcExpiry)
        {
            Contract.Ensures(Contract.Result<Task>() != null);
            Contract.Ensures(Contract.Result<Task>().Status != TaskStatus.Created);
            return TaskEx.Run(() => cache.AddTimed(partition, key, value, utcExpiry));
        }

        public static Task AddTimedAsync(this ICache cache, string key, object value, DateTime utcExpiry)
        {
            Contract.Ensures(Contract.Result<Task>() != null);
            Contract.Ensures(Contract.Result<Task>().Status != TaskStatus.Created);
            return TaskEx.Run(() => cache.AddTimed(cache.Settings.DefaultPartition, key, value, utcExpiry));
        }

        public static Task RemoveAsync(this ICache cache, string partition, string key)
        {
            Contract.Ensures(Contract.Result<Task>() != null);
            Contract.Ensures(Contract.Result<Task>().Status != TaskStatus.Created);
            return TaskEx.Run(() => cache.Remove(partition, key));
        }

        public static Task RemoveAsync(this ICache cache, string key)
        {
            Contract.Ensures(Contract.Result<Task>() != null);
            Contract.Ensures(Contract.Result<Task>().Status != TaskStatus.Created);
            return TaskEx.Run(() => cache.Remove(cache.Settings.DefaultPartition, key));
        }

        #endregion

        #region Typed Retrieval

        public static TObj Get<TObj>(this ICache cache, string partition, string key)
        {
            return (TObj) cache.Get(partition, key);
        }

        public static TObj Get<TObj>(this ICache cache, string key)
        {
            return (TObj) cache.Get(cache.Settings.DefaultPartition, key);
        }

        public static TObj Peek<TObj>(this ICache cache, string partition, string key)
        {
            return (TObj) cache.Peek(partition, key);
        }

        public static TObj Peek<TObj>(this ICache cache, string key)
        {
            return (TObj) cache.Peek(cache.Settings.DefaultPartition, key);
        }

        #endregion
    }
}
