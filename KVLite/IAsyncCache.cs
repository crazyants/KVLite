﻿// Copyright 2015-2025 Alessio Parma <alessio.parma@gmail.com>
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except
// in compliance with the License. You may obtain a copy of the License at:
//
// "http://www.apache.org/licenses/LICENSE-2.0"
//
// Unless required by applicable law or agreed to in writing, software distributed under the License
// is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express
// or implied. See the License for the specific language governing permissions and limitations under
// the License.

using PommaLabs.CodeServices.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Threading;
using System.Threading.Tasks;

namespace PommaLabs.KVLite
{
    /// <summary>
    ///   An async cache with specifically typed settings.
    /// </summary>
    /// <typeparam name="TSettings">The type of the async cache settings.</typeparam>
    public interface IAsyncCache<out TSettings> : IAsyncCache
        where TSettings : IAsyncCacheSettings
    {
        /// <summary>
        ///   The settings available for the async cache.
        /// </summary>
        /// <value>The settings available for the async cache.</value>
        [Pure]
        new TSettings Settings { get; }
    }

    /// <summary>
    ///   This interface represents a partition based key-value store. Each (partition, key, value)
    ///   triple has attached either an expiry time or a refresh interval, because values should not
    ///   be stored forever inside a cache.
    ///
    ///   In fact, a cache is, almost by definition, a transient store, used to temporaly store the
    ///   results of time consuming operations. This kind of cache should, therefore, store any kind
    ///   of object for a predetermined amount of time, trying to be extremely efficient while
    ///   handling entries. The cache does its best in order to be a reliable store, but it should
    ///   not be treated like a database: long story short, your code needs to be aware that values
    ///   in this cache may disappear as time passes.
    ///
    ///   In any case, to increase the ease of use, it is not mandatory neither to specify a
    ///   partition, nor to specify an expiration time. In both cases, a default value is used, which
    ///   can be customized by editing the KVLite configuration file. See, for example, the abstract
    ///   configuration of all caches (see <see cref="IEssentialCacheSettings.StaticIntervalInDays"/>).
    /// </summary>
    public interface IAsyncCache : IEssentialCache
    {
        /// <summary>
        ///   The settings available for the async cache.
        /// </summary>
        /// <value>The settings available for the async cache.</value>
        [Pure]
        IAsyncCacheSettings Settings { get; }

        /// <summary>
        ///   Gets the value with the specified partition and key.
        /// </summary>
        /// <value>The value with the specified partition and key.</value>
        /// <param name="partition">The partition.</param>
        /// <param name="key">The key.</param>
        /// <returns>The value with the specified partition and key.</returns>
        /// <exception cref="ArgumentNullException">
        ///   <paramref name="partition"/> or <paramref name="key"/> are null.
        /// </exception>
        /// <remarks>
        ///   This method, differently from other readers (like
        ///   <see cref="GetAsync{TVal}(string,string,CancellationToken)"/> or
        ///   <see cref="PeekAsync{TVal}(string,string,CancellationToken)"/>), does not have a typed
        ///   return object, because indexers cannot be generic. Therefore, we have to return a
        ///   simple <see cref="object"/>.
        /// </remarks>
        [Pure]
        Task<Option<object>> this[string partition, string key] { get; }

        /// <summary>
        ///   Computes cache size in bytes. This value might be an estimate of real cache size and,
        ///   therefore, it does not need to be extremely accurate.
        /// </summary>
        /// <param name="cancellationToken">An optional cancellation token.</param>
        /// <returns>An estimate of cache size in bytes.</returns>
        [Pure]
        Task<long> GetCacheSizeInBytesAsync(CancellationToken cancellationToken = default(CancellationToken));

        #region Add

        /// <summary>
        ///   Adds a "sliding" value with given partition and key. Value will last as much as
        ///   specified in given interval and, if accessed before expiry, its lifetime will be
        ///   extended by the interval itself.
        /// </summary>
        /// <typeparam name="TVal">The type of the value.</typeparam>
        /// <param name="partition">The partition.</param>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <param name="interval">The interval.</param>
        /// <param name="parentKeys">
        ///   Keys, belonging to current partition, on which the new item will depend.
        /// </param>
        /// <param name="cancellationToken">An optional cancellation token.</param>
        /// <exception cref="ArgumentNullException">
        ///   <paramref name="partition"/> or <paramref name="key"/> are null.
        /// </exception>
        /// <exception cref="NotSupportedException">
        ///   Too many parent keys have been specified for this item. Please have a look at the
        ///   <see cref="IEssentialCache.MaxParentKeyCountPerItem"/> to understand how many parent
        ///   keys each item may have.
        /// </exception>
        Task AddSlidingAsync<TVal>(string partition, string key, TVal value, TimeSpan interval, IList<string> parentKeys = null, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        ///   Adds a "static" value with given partition and key. Value will last as much as
        ///   specified in <see cref="IEssentialCacheSettings.StaticIntervalInDays"/> and, if
        ///   accessed before expiry, its lifetime will be extended by that interval.
        /// </summary>
        /// <typeparam name="TVal">The type of the value.</typeparam>
        /// <param name="partition">The partition.</param>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <param name="parentKeys">
        ///   Keys, belonging to current partition, on which the new item will depend.
        /// </param>
        /// <param name="cancellationToken">An optional cancellation token.</param>
        /// <exception cref="ArgumentNullException">
        ///   <paramref name="partition"/> or <paramref name="key"/> are null.
        /// </exception>
        /// <exception cref="NotSupportedException">
        ///   Too many parent keys have been specified for this item. Please have a look at the
        ///   <see cref="IEssentialCache.MaxParentKeyCountPerItem"/> to understand how many parent
        ///   keys each item may have.
        /// </exception>
        Task AddStaticAsync<TVal>(string partition, string key, TVal value, IList<string> parentKeys = null, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        ///   Adds a "timed" value with given partition and key. Value will last until the specified
        ///   time and, if accessed before expiry, its lifetime will _not_ be extended.
        /// </summary>
        /// <typeparam name="TVal">The type of the value.</typeparam>
        /// <param name="partition">The partition.</param>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <param name="utcExpiry">The UTC expiry.</param>
        /// <param name="parentKeys">
        ///   Keys, belonging to current partition, on which the new item will depend.
        /// </param>
        /// <param name="cancellationToken">An optional cancellation token.</param>
        /// <exception cref="ArgumentNullException">
        ///   <paramref name="partition"/> or <paramref name="key"/> are null.
        /// </exception>
        /// <exception cref="NotSupportedException">
        ///   Too many parent keys have been specified for this item. Please have a look at the
        ///   <see cref="IEssentialCache.MaxParentKeyCountPerItem"/> to understand how many parent
        ///   keys each item may have.
        /// </exception>
        Task AddTimedAsync<TVal>(string partition, string key, TVal value, DateTime utcExpiry, IList<string> parentKeys = null, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        ///   Adds a "timed" value with given partition and key. Value will last for the specified
        ///   lifetime and, if accessed before expiry, its lifetime will _not_ be extended.
        /// </summary>
        /// <typeparam name="TVal">The type of the value.</typeparam>
        /// <param name="partition">The partition.</param>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <param name="lifetime">The desired lifetime.</param>
        /// <param name="parentKeys">
        ///   Keys, belonging to current partition, on which the new item will depend.
        /// </param>
        /// <param name="cancellationToken">An optional cancellation token.</param>
        /// <exception cref="ArgumentNullException">
        ///   <paramref name="partition"/> or <paramref name="key"/> are null.
        /// </exception>
        /// <exception cref="NotSupportedException">
        ///   Too many parent keys have been specified for this item. Please have a look at the
        ///   <see cref="IEssentialCache.MaxParentKeyCountPerItem"/> to understand how many parent
        ///   keys each item may have.
        /// </exception>
        Task AddTimedAsync<TVal>(string partition, string key, TVal value, TimeSpan lifetime, IList<string> parentKeys = null, CancellationToken cancellationToken = default(CancellationToken));

        #endregion Add

        #region Clear

        /// <summary>
        ///   Clears this instance, that is, it removes all stored items.
        /// </summary>
        /// <param name="cancellationToken">An optional cancellation token.</param>
        /// <returns>The number of items that have been removed.</returns>
        Task<long> ClearAsync(CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        ///   Clears given partition, that is, it removes all its items.
        /// </summary>
        /// <param name="partition">The partition.</param>
        /// <param name="cancellationToken">An optional cancellation token.</param>
        /// <returns>The number of items that have been removed.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="partition"/> is null.</exception>
        Task<long> ClearAsync(string partition, CancellationToken cancellationToken = default(CancellationToken));

        #endregion Clear

        #region Count

        /// <summary>
        ///   The number of items stored in the cache.
        /// </summary>
        /// <param name="cancellationToken">An optional cancellation token.</param>
        /// <returns>The number of items stored in the cache.</returns>
        /// <remarks>Calling this method does not extend sliding items lifetime.</remarks>
        [Pure]
        Task<int> CountAsync(CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        ///   The number of items stored in given partition.
        /// </summary>
        /// <param name="partition">The partition.</param>
        /// <param name="cancellationToken">An optional cancellation token.</param>
        /// <returns>The number of items stored in given partition.</returns>
        /// <remarks>Calling this method does not extend sliding items lifetime.</remarks>
        /// <exception cref="ArgumentNullException"><paramref name="partition"/> is null.</exception>
        [Pure]
        Task<int> CountAsync(string partition, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        ///   The number of items stored in the cache.
        /// </summary>
        /// <param name="cancellationToken">An optional cancellation token.</param>
        /// <returns>The number of items stored in the cache.</returns>
        /// <remarks>Calling this method does not extend sliding items lifetime.</remarks>
        [Pure]
        Task<long> LongCountAsync(CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        ///   The number of items stored in given partition.
        /// </summary>
        /// <param name="partition">The partition.</param>
        /// <param name="cancellationToken">An optional cancellation token.</param>
        /// <returns>The number of items stored in given partition.</returns>
        /// <remarks>Calling this method does not extend sliding items lifetime.</remarks>
        /// <exception cref="ArgumentNullException"><paramref name="partition"/> is null.</exception>
        [Pure]
        Task<long> LongCountAsync(string partition, CancellationToken cancellationToken = default(CancellationToken));

        #endregion Count

        #region Contains

        /// <summary>
        ///   Determines whether this cache contains the specified partition and key.
        /// </summary>
        /// <param name="partition">The partition.</param>
        /// <param name="key">The key.</param>
        /// <param name="cancellationToken">An optional cancellation token.</param>
        /// <returns>Whether this cache contains the specified partition and key.</returns>
        /// <remarks>Calling this method does not extend sliding items lifetime.</remarks>
        /// <exception cref="ArgumentNullException">
        ///   <paramref name="partition"/> or <paramref name="key"/> are null.
        /// </exception>
        [Pure]
        Task<bool> ContainsAsync(string partition, string key, CancellationToken cancellationToken = default(CancellationToken));

        #endregion Contains

        #region Get & GetItem(s)

        /// <summary>
        ///   Gets the value with specified partition and key. If it is a "sliding" or "static"
        ///   value, its lifetime will be increased by the corresponding interval.
        /// </summary>
        /// <param name="partition">The partition.</param>
        /// <param name="key">The key.</param>
        /// <param name="cancellationToken">An optional cancellation token.</param>
        /// <typeparam name="TVal">The type of the expected value.</typeparam>
        /// <returns>The value with specified partition and key.</returns>
        /// <remarks>
        ///   If you are uncertain of which type the value should have, you can always pass
        ///   <see cref="object"/> as type parameter; that will work whether the required value is a
        ///   class or not.
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        ///   <paramref name="partition"/> or <paramref name="key"/> are null.
        /// </exception>
        Task<Option<TVal>> GetAsync<TVal>(string partition, string key, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        ///   Gets the cache item with specified partition and key. If it is a "sliding" or "static"
        ///   value, its lifetime will be increased by corresponding interval.
        /// </summary>
        /// <param name="partition">The partition.</param>
        /// <param name="key">The key.</param>
        /// <param name="cancellationToken">An optional cancellation token.</param>
        /// <typeparam name="TVal">The type of the expected value.</typeparam>
        /// <returns>The cache item with specified partition and key.</returns>
        /// <remarks>
        ///   If you are uncertain of which type the value should have, you can always pass
        ///   <see cref="object"/> as type parameter; that will work whether the required value is a
        ///   class or not.
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        ///   <paramref name="partition"/> or <paramref name="key"/> are null.
        /// </exception>
        Task<Option<ICacheItem<TVal>>> GetItemAsync<TVal>(string partition, string key, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        ///   Gets all cache items. If an item is a "sliding" or "static" value, its lifetime will be
        ///   increased by corresponding interval.
        /// </summary>
        /// <param name="cancellationToken">An optional cancellation token.</param>
        /// <typeparam name="TVal">The type of the expected values.</typeparam>
        /// <returns>All cache items.</returns>
        /// <remarks>
        ///   If you are uncertain of which type the value should have, you can always pass
        ///   <see cref="object"/> as type parameter; that will work whether the required value is a
        ///   class or not.
        /// </remarks>
        Task<IList<ICacheItem<TVal>>> GetItemsAsync<TVal>(CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        ///   Gets all cache items in given partition. If an item is a "sliding" or "static" value,
        ///   its lifetime will be increased by corresponding interval.
        /// </summary>
        /// <param name="partition">The partition.</param>
        /// <param name="cancellationToken">An optional cancellation token.</param>
        /// <typeparam name="TVal">The type of the expected values.</typeparam>
        /// <returns>All cache items in given partition.</returns>
        /// <remarks>
        ///   If you are uncertain of which type the value should have, you can always pass
        ///   <see cref="object"/> as type parameter; that will work whether the required value is a
        ///   class or not.
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="partition"/> is null.</exception>
        Task<IList<ICacheItem<TVal>>> GetItemsAsync<TVal>(string partition, CancellationToken cancellationToken = default(CancellationToken));

        #endregion Get & GetItem(s)

        #region GetOrAdd

        /// <summary>
        ///   At first, it tries to get the cache item with specified partition and key. If it is a
        ///   "sliding" or "static" value, its lifetime will be increased by corresponding interval.
        ///
        ///   If the value is not found, then it adds a "sliding" value with given partition and key.
        ///   Value will last as much as specified in given interval and, if accessed before expiry,
        ///   its lifetime will be extended by the interval itself.
        /// </summary>
        /// <typeparam name="TVal">The type of the value.</typeparam>
        /// <param name="partition">The partition.</param>
        /// <param name="key">The key.</param>
        /// <param name="valueGetter">
        ///   The function that is called in order to get the value when it was not found inside the cache.
        /// </param>
        /// <param name="interval">The interval.</param>
        /// <param name="parentKeys">
        ///   Keys, belonging to current partition, on which the new item will depend.
        /// </param>
        /// <param name="cancellationToken">An optional cancellation token.</param>
        /// <returns>
        ///   The value found in the cache or the one returned by <paramref name="valueGetter"/>, in
        ///   case a new value has been added to the cache.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///   <paramref name="partition"/>, <paramref name="key"/> or <paramref name="valueGetter"/>
        ///   are null.
        /// </exception>
        /// <exception cref="NotSupportedException">
        ///   Too many parent keys have been specified for this item. Please have a look at the
        ///   <see cref="IEssentialCache.MaxParentKeyCountPerItem"/> to understand how many parent
        ///   keys each item may have.
        /// </exception>
        Task<TVal> GetOrAddSlidingAsync<TVal>(string partition, string key, Func<Task<TVal>> valueGetter, TimeSpan interval, IList<string> parentKeys = null, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        ///   At first, it tries to get the cache item with specified partition and key. If it is a
        ///   "sliding" or "static" value, its lifetime will be increased by corresponding interval.
        ///
        ///   If the value is not found, then it adds a "static" value with given partition and key.
        ///   Value will last as much as specified in
        ///   <see cref="IEssentialCacheSettings.StaticIntervalInDays"/> and, if accessed before
        ///   expiry, its lifetime will be extended by that interval.
        /// </summary>
        /// <typeparam name="TVal">The type of the value.</typeparam>
        /// <param name="partition">The partition.</param>
        /// <param name="key">The key.</param>
        /// <param name="valueGetter">
        ///   The function that is called in order to get the value when it was not found inside the cache.
        /// </param>
        /// <param name="parentKeys">
        ///   Keys, belonging to current partition, on which the new item will depend.
        /// </param>
        /// <param name="cancellationToken">An optional cancellation token.</param>
        /// <returns>
        ///   The value found in the cache or the one returned by <paramref name="valueGetter"/>, in
        ///   case a new value has been added to the cache.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///   <paramref name="partition"/>, <paramref name="key"/> or <paramref name="valueGetter"/>
        ///   are null.
        /// </exception>
        /// <exception cref="NotSupportedException">
        ///   Too many parent keys have been specified for this item. Please have a look at the
        ///   <see cref="IEssentialCache.MaxParentKeyCountPerItem"/> to understand how many parent
        ///   keys each item may have.
        /// </exception>
        Task<TVal> GetOrAddStaticAsync<TVal>(string partition, string key, Func<Task<TVal>> valueGetter, IList<string> parentKeys = null, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        ///   At first, it tries to get the cache item with specified partition and key. If it is a
        ///   "sliding" or "static" value, its lifetime will be increased by corresponding interval.
        ///
        ///   If the value is not found, then it adds a "timed" value with given partition and key.
        ///   Value will last until the specified time and, if accessed before expiry, its lifetime
        ///   will _not_ be extended.
        /// </summary>
        /// <typeparam name="TVal">The type of the value.</typeparam>
        /// <param name="partition">The partition.</param>
        /// <param name="key">The key.</param>
        /// <param name="valueGetter">
        ///   The function that is called in order to get the value when it was not found inside the cache.
        /// </param>
        /// <param name="utcExpiry">The UTC expiry.</param>
        /// <param name="parentKeys">
        ///   Keys, belonging to current partition, on which the new item will depend.
        /// </param>
        /// <param name="cancellationToken">An optional cancellation token.</param>
        /// <returns>
        ///   The value found in the cache or the one returned by <paramref name="valueGetter"/>, in
        ///   case a new value has been added to the cache.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///   <paramref name="partition"/>, <paramref name="key"/> or <paramref name="valueGetter"/>
        ///   are null.
        /// </exception>
        /// <exception cref="NotSupportedException">
        ///   Too many parent keys have been specified for this item. Please have a look at the
        ///   <see cref="IEssentialCache.MaxParentKeyCountPerItem"/> to understand how many parent
        ///   keys each item may have.
        /// </exception>
        Task<TVal> GetOrAddTimedAsync<TVal>(string partition, string key, Func<Task<TVal>> valueGetter, DateTime utcExpiry, IList<string> parentKeys = null, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        ///   At first, it tries to get the cache item with specified partition and key. If it is a
        ///   "sliding" or "static" value, its lifetime will be increased by corresponding interval.
        ///
        ///   If the value is not found, then it adds a "timed" value with given partition and key.
        ///   Value will last for the specified lifetime and, if accessed before expiry, its lifetime
        ///   will _not_ be extended.
        /// </summary>
        /// <typeparam name="TVal">The type of the value.</typeparam>
        /// <param name="partition">The partition.</param>
        /// <param name="key">The key.</param>
        /// <param name="valueGetter">
        ///   The function that is called in order to get the value when it was not found inside the cache.
        /// </param>
        /// <param name="lifetime">The desired lifetime.</param>
        /// <param name="parentKeys">
        ///   Keys, belonging to current partition, on which the new item will depend.
        /// </param>
        /// <param name="cancellationToken">An optional cancellation token.</param>
        /// <returns>
        ///   The value found in the cache or the one returned by <paramref name="valueGetter"/>, in
        ///   case a new value has been added to the cache.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///   <paramref name="partition"/>, <paramref name="key"/> or <paramref name="valueGetter"/>
        ///   are null.
        /// </exception>
        /// <exception cref="NotSupportedException">
        ///   Too many parent keys have been specified for this item. Please have a look at the
        ///   <see cref="IEssentialCache.MaxParentKeyCountPerItem"/> to understand how many parent
        ///   keys each item may have.
        /// </exception>
        Task<TVal> GetOrAddTimedAsync<TVal>(string partition, string key, Func<Task<TVal>> valueGetter, TimeSpan lifetime, IList<string> parentKeys = null, CancellationToken cancellationToken = default(CancellationToken));

        #endregion GetOrAdd

        #region Peek & PeekItem(s)

        /// <summary>
        ///   Gets the value corresponding to given partition and key, without updating expiry date.
        /// </summary>
        /// <param name="partition">The partition.</param>
        /// <param name="key">The key.</param>
        /// <param name="cancellationToken">An optional cancellation token.</param>
        /// <typeparam name="TVal">The type of the expected values.</typeparam>
        /// <returns>
        ///   The value corresponding to given partition and key, without updating expiry date.
        /// </returns>
        /// <remarks>
        ///   If you are uncertain of which type the value should have, you can always pass
        ///   <see cref="object"/> as type parameter; that will work whether the required value is a
        ///   class or not.
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        ///   <paramref name="partition"/> or <paramref name="key"/> are null.
        /// </exception>
        /// <exception cref="NotSupportedException">
        ///   Cache does not support peeking (please have a look at the
        ///   <see cref="IEssentialCache.CanPeek"/> property).
        /// </exception>
        [Pure]
        Task<Option<TVal>> PeekAsync<TVal>(string partition, string key, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        ///   Gets the item corresponding to given partition and key, without updating expiry date.
        /// </summary>
        /// <param name="partition">The partition.</param>
        /// <param name="key">The key.</param>
        /// <param name="cancellationToken">An optional cancellation token.</param>
        /// <typeparam name="TVal">The type of the expected values.</typeparam>
        /// <returns>
        ///   The item corresponding to given partition and key, without updating expiry date.
        /// </returns>
        /// <remarks>
        ///   If you are uncertain of which type the value should have, you can always pass
        ///   <see cref="object"/> as type parameter; that will work whether the required value is a
        ///   class or not.
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        ///   <paramref name="partition"/> or <paramref name="key"/> are null.
        /// </exception>
        /// <exception cref="NotSupportedException">
        ///   Cache does not support peeking (please have a look at the
        ///   <see cref="IEssentialCache.CanPeek"/> property).
        /// </exception>
        [Pure]
        Task<Option<ICacheItem<TVal>>> PeekItemAsync<TVal>(string partition, string key, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        ///   Gets the all values, without updating expiry dates.
        /// </summary>
        /// <param name="cancellationToken">An optional cancellation token.</param>
        /// <typeparam name="TVal">The type of the expected values.</typeparam>
        /// <returns>All values, without updating expiry dates.</returns>
        /// <remarks>
        ///   If you are uncertain of which type the value should have, you can always pass
        ///   <see cref="object"/> as type parameter; that will work whether the required value is a
        ///   class or not.
        /// </remarks>
        /// <exception cref="NotSupportedException">
        ///   Cache does not support peeking (please have a look at the
        ///   <see cref="IEssentialCache.CanPeek"/> property).
        /// </exception>
        [Pure]
        Task<IList<ICacheItem<TVal>>> PeekItemsAsync<TVal>(CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        ///   Gets the all items in given partition, without updating expiry dates.
        /// </summary>
        /// <param name="partition">The partition.</param>
        /// <param name="cancellationToken">An optional cancellation token.</param>
        /// <typeparam name="TVal">The type of the expected values.</typeparam>
        /// <returns>All items in given partition, without updating expiry dates.</returns>
        /// <remarks>
        ///   If you are uncertain of which type the value should have, you can always pass
        ///   <see cref="object"/> as type parameter; that will work whether the required value is a
        ///   class or not.
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="partition"/> is null.</exception>
        /// <exception cref="NotSupportedException">
        ///   Cache does not support peeking (please have a look at the
        ///   <see cref="IEssentialCache.CanPeek"/> property).
        /// </exception>
        [Pure]
        Task<IList<ICacheItem<TVal>>> PeekItemsAsync<TVal>(string partition, CancellationToken cancellationToken = default(CancellationToken));

        #endregion Peek & PeekItem(s)

        #region Remove

        /// <summary>
        ///   Removes the item with given partition and key.
        /// </summary>
        /// <param name="partition">The partition.</param>
        /// <param name="key">The key.</param>
        /// <param name="cancellationToken">An optional cancellation token.</param>
        /// <exception cref="ArgumentNullException">
        ///   <paramref name="partition"/> or <paramref name="key"/> are null.
        /// </exception>
        Task RemoveAsync(string partition, string key, CancellationToken cancellationToken = default(CancellationToken));

        #endregion Remove
    }
}