﻿// File name: ICache.cs
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
using System.Diagnostics.Contracts;
using Finsa.CodeServices.Clock;
using PommaLabs.KVLite.Contracts;
using PommaLabs.KVLite.Core;

namespace PommaLabs.KVLite
{
    /// <summary>
    ///   Represents a partition based key-value store. Each (partition, key, value) triple has
    ///   attached either an expiry time or a refresh interval, because values should not be stored
    ///   forever inside a cache. <br/> In fact, a cache is, almost by definition, a transient
    ///   store, used to temporaly store the results of time consuming operations.
    /// </summary>
    [ContractClass(typeof(CacheContract))]
    public interface ICache
    {
        /// <summary>
        ///   Gets the clock used by the cache.
        /// </summary>
        /// <value>
        ///   The clock used by the cache.
        /// </value>
        IClock Clock { get; }

        /// <summary>
        ///   The available settings for the cache.
        /// </summary>
        CacheSettingsBase Settings { get; }

        /// <summary>
        ///   Gets the value with the specified partition and key.
        /// </summary>
        /// <value>The value with the specified partition and key.</value>
        /// <param name="partition">The partition.</param>
        /// <param name="key">The key.</param>
        /// <returns>The value with the specified partition and key.</returns>
        [Pure]
        object this[string partition, string key] { get; }

        /// <summary>
        ///   Adds a "sliding" value with given partition and key. Value will last as much as
        ///   specified in given interval and, if accessed before expiry, its lifetime will be
        ///   extended by the interval itself.
        /// </summary>
        /// <param name="partition">The partition.</param>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <param name="interval">The interval.</param>
        void AddSliding(string partition, string key, object value, TimeSpan interval);

        /// <summary>
        ///   Adds a "timed" value with given partition and key. Value will last until the specified
        ///   time and, if accessed before expiry, its lifetime will _not_ be extended.
        /// </summary>
        /// <param name="partition">The partition.</param>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <param name="utcExpiry">The UTC expiry.</param>
        void AddTimed(string partition, string key, object value, DateTime utcExpiry);

        /// <summary>
        ///   Clears this instance, that is, it removes all values.
        /// </summary>
        void Clear();

        /// <summary>
        ///   Clears given partition, that is, it removes all its values.
        /// </summary>
        void Clear(string partition);

        /// <summary>
        ///   Determines whether cache contains the specified partition and key.
        /// </summary>
        /// <param name="partition">The partition.</param>
        /// <param name="key">The key.</param>
        /// <returns>Whether cache contains the specified partition and key.</returns>
        [Pure]
        bool Contains(string partition, string key);

        /// <summary>
        ///   The number of items in the cache.
        /// </summary>
        /// <returns>The number of items in the cache.</returns>
        [Pure]
        long LongCount();

        /// <summary>
        ///   The number of items in given partition.
        /// </summary>
        /// <returns>The number of items in given partition.</returns>
        [Pure]
        long LongCount(string partition);

        /// <summary>
        ///   Gets the value with specified partition and key. If it is a "sliding" or "static"
        ///   value, its lifetime will be increased by corresponding interval.
        /// </summary>
        /// <param name="partition">The partition.</param>
        /// <param name="key">The key.</param>
        /// <returns>The value with specified partition and key.</returns>
        object Get(string partition, string key);

        /// <summary>
        ///   Gets the cache item with specified partition and key. If it is a "sliding" or "static"
        ///   value, its lifetime will be increased by corresponding interval.
        /// </summary>
        /// <param name="partition">The partition.</param>
        /// <param name="key">The key.</param>
        /// <returns>The cache item with specified partition and key.</returns>
        CacheItem GetItem(string partition, string key);

        /// <summary>
        ///   Gets all cache items. If an item is a "sliding" or "static" value, its lifetime will
        ///   be increased by corresponding interval.
        /// </summary>
        /// <returns>All cache items.</returns>
        IList<CacheItem> GetManyItems();

        /// <summary>
        ///   Gets all cache items in given partition. If an item is a "sliding" or "static" value,
        ///   its lifetime will be increased by corresponding interval.
        /// </summary>
        /// <param name="partition">The partition.</param>
        /// <returns>All cache items in given partition.</returns>
        IList<CacheItem> GetManyItems(string partition);

        /// <summary>
        ///   Gets the value corresponding to given partition and key, without updating expiry date.
        /// </summary>
        /// <returns>
        ///   The value corresponding to given partition and key, without updating expiry date.
        /// </returns>
        [Pure]
        object Peek(string partition, string key);

        /// <summary>
        ///   Gets the item corresponding to given partition and key, without updating expiry date.
        /// </summary>
        /// <returns>
        ///   The item corresponding to given partition and key, without updating expiry date.
        /// </returns>
        [Pure]
        CacheItem PeekItem(string partition, string key);

        /// <summary>
        ///   Gets the all values, without updating expiry dates.
        /// </summary>
        /// <returns>All values, without updating expiry dates.</returns>
        [Pure]
        IList<CacheItem> PeekManyItems();

        /// <summary>
        ///   Gets the all items in given partition, without updating expiry dates.
        /// </summary>
        /// <returns>All items in given partition, without updating expiry dates.</returns>
        [Pure]
        IList<CacheItem> PeekManyItems(string partition);

        /// <summary>
        ///   Removes the value with given partition and key.
        /// </summary>
        /// <param name="partition">The partition.</param>
        /// <param name="key">The key.</param>
        void Remove(string partition, string key);
    }

    /// <summary>
    ///   A cache with specifically typed settings.
    /// </summary>
    /// <typeparam name="TCacheSettings">The type of the cache settings.</typeparam>
    public interface ICache<out TCacheSettings> : ICache
        where TCacheSettings : CacheSettingsBase
    {
        /// <summary>
        ///   The available settings for the cache.
        /// </summary>
        new TCacheSettings Settings { get; }
    }
}