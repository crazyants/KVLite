﻿// File name: DbCache.cs
//
// Author(s): Alessio Parma <alessio.parma@gmail.com>
//
// The MIT License (MIT)
//
// Copyright (c) 2014-2016 Alessio Parma <alessio.parma@gmail.com>
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
// DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT
// OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using CodeProject.ObjectPool.Specialized;
using Common.Logging;
using PommaLabs.CodeServices.Caching;
using PommaLabs.CodeServices.Clock;
using PommaLabs.CodeServices.Common;
using PommaLabs.CodeServices.Common.Collections.Generic;
using PommaLabs.CodeServices.Compression;
using PommaLabs.CodeServices.Serialization;
using PommaLabs.KVLite.Core;
using PommaLabs.Thrower;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Core;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Text;
using System.Transactions;

namespace PommaLabs.KVLite
{
    /// <summary>
    ///   Base class for SQL caches, implements common functionalities.
    /// </summary>
    /// <typeparam name="TSettings">The type of the cache settings.</typeparam>
    public class DbCache<TSettings> : AbstractCache<TSettings>
        where TSettings : DbCacheSettings<TSettings>
    {
        #region Construction

        /// <summary>
        ///   Initializes a new instance of the <see cref="DbCache{TSettings}"/> class with given settings.
        /// </summary>
        /// <param name="settings">The settings.</param>
        /// <param name="connectionFactory">The DB connection factory.</param>
        /// <param name="clock">The clock.</param>
        /// <param name="log">The log.</param>
        /// <param name="serializer">The serializer.</param>
        /// <param name="compressor">The compressor.</param>
        /// <param name="memoryStreamPool">The memory stream pool.</param>
        public DbCache(TSettings settings, IDbCacheConnectionFactory connectionFactory, IClock clock, ILog log, ISerializer serializer, ICompressor compressor, IMemoryStreamPool memoryStreamPool)
        {
            // Preconditions
            Raise.ArgumentNullException.IfIsNull(settings, nameof(settings), ErrorMessages.NullSettings);
            Raise.ArgumentNullException.IfIsNull(connectionFactory, nameof(connectionFactory));

            Settings = settings;
            Settings.ConnectionFactory = connectionFactory;
            Clock = clock ?? Constants.DefaultClock;
            Log = log ?? LogManager.GetLogger(GetType());
            Serializer = serializer ?? Constants.DefaultSerializer;
            Compressor = compressor ?? Constants.DefaultCompressor;
            MemoryStreamPool = memoryStreamPool ?? Constants.DefaultMemoryStreamPool;
        }

        #endregion Construction

        #region Public Members

        /// <summary>
        ///   The connection factory used to retrieve connections to the cache data store.
        /// </summary>
        public IDbCacheConnectionFactory ConnectionFactory => Settings.ConnectionFactory;

        /// <summary>
        ///   Returns current cache size in kilobytes.
        /// </summary>
        /// <returns>Current cache size in kilobytes.</returns>
        [Pure]
        public long CacheSizeInKB()
        {
            // Preconditions
            Raise.ObjectDisposedException.If(Disposed, nameof(ICache), ErrorMessages.CacheHasBeenDisposed);

            try
            {
                return ConnectionFactory.GetCacheSizeInKB();
            }
            catch (Exception ex)
            {
                LastError = ex;
                Log.Error(ErrorMessages.InternalErrorOnReadAll, ex);
                return 0L;
            }
        }

        /// <summary>
        ///   Clears the cache using the specified cache read mode.
        /// </summary>
        /// <param name="cacheReadMode">The cache read mode.</param>
        /// <returns>The number of items that have been removed.</returns>
        public long Clear(CacheReadMode cacheReadMode)
        {
            // Preconditions
            Raise.ObjectDisposedException.If(Disposed, nameof(ICache), ErrorMessages.CacheHasBeenDisposed);
            Raise.ArgumentException.IfNot(Enum.IsDefined(typeof(CacheReadMode), cacheReadMode), nameof(cacheReadMode), ErrorMessages.InvalidCacheReadMode);

            try
            {
                var result = ClearInternal(null, cacheReadMode);

                // Postconditions - NOT VALID: Methods below return counters which are not related to
                // the number of items the call above actually cleared.

                //Debug.Assert(Count(cacheReadMode) == 0);
                //Debug.Assert(LongCount(cacheReadMode) == 0L);
                Debug.Assert(result >= 0L);
                return result;
            }
            catch (Exception ex)
            {
                LastError = ex;
                Log.Error(ErrorMessages.InternalErrorOnClearAll, ex);
                return 0L;
            }
        }

        /// <summary>
        ///   Clears the specified partition using the specified cache read mode.
        /// </summary>
        /// <param name="partition">The partition.</param>
        /// <param name="cacheReadMode">The cache read mode.</param>
        /// <returns>The number of items that have been removed.</returns>
        public long Clear(string partition, CacheReadMode cacheReadMode)
        {
            // Preconditions
            Raise.ObjectDisposedException.If(Disposed, nameof(ICache), ErrorMessages.CacheHasBeenDisposed);
            Raise.ArgumentNullException.IfIsNull(partition, nameof(partition), ErrorMessages.NullPartition);
            Raise.ArgumentException.IfNot(Enum.IsDefined(typeof(CacheReadMode), cacheReadMode), nameof(cacheReadMode), ErrorMessages.InvalidCacheReadMode);

            try
            {
                var result = ClearInternal(partition, cacheReadMode);

                // Postconditions - NOT VALID: Methods below return counters which are not related to
                // the number of items the call above actually cleared.

                //Debug.Assert(Count(cacheReadMode) == 0);
                //Debug.Assert(LongCount(cacheReadMode) == 0L);
                Debug.Assert(result >= 0L);
                return result;
            }
            catch (Exception ex)
            {
                LastError = ex;
                Log.Error(string.Format(ErrorMessages.InternalErrorOnClearPartition, partition), ex);
                return 0L;
            }
        }

        /// <summary>
        ///   The number of items in the cache.
        /// </summary>
        /// <param name="cacheReadMode">Whether invalid items should be included in the count.</param>
        /// <returns>The number of items in the cache.</returns>
        [Pure]
        public int Count(CacheReadMode cacheReadMode)
        {
            // Preconditions
            Raise.ObjectDisposedException.If(Disposed, nameof(ICache), ErrorMessages.CacheHasBeenDisposed);
            Raise.ArgumentException.IfNot(Enum.IsDefined(typeof(CacheReadMode), cacheReadMode), nameof(cacheReadMode), ErrorMessages.InvalidCacheReadMode);

            try
            {
                var result = Convert.ToInt32(CountInternal(null, cacheReadMode));

                // Postconditions
                Debug.Assert(result >= 0);
                return result;
            }
            catch (Exception ex)
            {
                LastError = ex;
                Log.Error(ErrorMessages.InternalErrorOnCountAll, ex);
                return 0;
            }
        }

        /// <summary>
        ///   The number of items in the cache for given partition.
        /// </summary>
        /// <param name="partition">The partition.</param>
        /// <param name="cacheReadMode">Whether invalid items should be included in the count.</param>
        /// <returns>The number of items in the cache.</returns>
        [Pure]
        public int Count(string partition, CacheReadMode cacheReadMode)
        {
            // Preconditions
            Raise.ObjectDisposedException.If(Disposed, nameof(ICache), ErrorMessages.CacheHasBeenDisposed);
            Raise.ArgumentNullException.IfIsNull(partition, nameof(partition), ErrorMessages.NullPartition);
            Raise.ArgumentException.IfNot(Enum.IsDefined(typeof(CacheReadMode), cacheReadMode), nameof(cacheReadMode), ErrorMessages.InvalidCacheReadMode);

            try
            {
                var result = Convert.ToInt32(CountInternal(partition, cacheReadMode));

                // Postconditions
                Debug.Assert(result >= 0);
                return result;
            }
            catch (Exception ex)
            {
                LastError = ex;
                Log.Error(string.Format(ErrorMessages.InternalErrorOnCountPartition, partition), ex);
                return 0;
            }
        }

        /// <summary>
        ///   The number of items in the cache.
        /// </summary>
        /// <param name="cacheReadMode">Whether invalid items should be included in the count.</param>
        /// <returns>The number of items in the cache.</returns>
        [Pure]
        public long LongCount(CacheReadMode cacheReadMode)
        {
            // Preconditions
            Raise.ObjectDisposedException.If(Disposed, nameof(ICache), ErrorMessages.CacheHasBeenDisposed);
            Raise.ArgumentException.IfNot(Enum.IsDefined(typeof(CacheReadMode), cacheReadMode), nameof(cacheReadMode), ErrorMessages.InvalidCacheReadMode);

            try
            {
                var result = CountInternal(null, cacheReadMode);

                // Postconditions
                Debug.Assert(result >= 0L);
                return result;
            }
            catch (Exception ex)
            {
                LastError = ex;
                Log.Error(ErrorMessages.InternalErrorOnCountAll, ex);
                return 0L;
            }
        }

        /// <summary>
        ///   The number of items in the cache for given partition.
        /// </summary>
        /// <param name="partition">The partition.</param>
        /// <param name="cacheReadMode">Whether invalid items should be included in the count.</param>
        /// <returns>The number of items in the cache.</returns>
        [Pure]
        public long LongCount(string partition, CacheReadMode cacheReadMode)
        {
            // Preconditions
            Raise.ObjectDisposedException.If(Disposed, nameof(ICache), ErrorMessages.CacheHasBeenDisposed);
            Raise.ArgumentNullException.IfIsNull(partition, nameof(partition), ErrorMessages.NullPartition);
            Raise.ArgumentException.IfNot(Enum.IsDefined(typeof(CacheReadMode), cacheReadMode), nameof(cacheReadMode), ErrorMessages.InvalidCacheReadMode);

            try
            {
                var result = CountInternal(partition, cacheReadMode);

                // Postconditions
                Debug.Assert(result >= 0L);
                return result;
            }
            catch (Exception ex)
            {
                LastError = ex;
                Log.Error(string.Format(ErrorMessages.InternalErrorOnCountPartition, partition), ex);
                return 0L;
            }
        }

        #endregion Public Members

        #region FormattableObject members

        /// <summary>
        ///   Returns all property (or field) values, along with their names, so that they can be
        ///   used to produce a meaningful <see cref="FormattableObject.ToString"/>.
        /// </summary>
        /// <returns>
        ///   Returns all property (or field) values, along with their names, so that they can be
        ///   used to produce a meaningful <see cref="FormattableObject.ToString"/>.
        /// </returns>
        protected override IEnumerable<KeyValuePair<string, string>> GetFormattingMembers()
        {
            yield return KeyValuePair.Create(nameof(Settings.CacheUri), Settings.CacheUri);
        }

        #endregion FormattableObject members

        #region IDisposable members

        /// <summary>
        ///   Performs application-defined tasks associated with freeing, releasing, or resetting
        ///   unmanaged resources.
        /// </summary>
        /// <param name="disposing">True if it is a managed dispose, false otherwise.</param>
        protected override void Dispose(bool disposing)
        {
            if (!disposing)
            {
                // Nothing to do, we can handle only managed Dispose calls.
                return;
            }
        }

        #endregion IDisposable members

        #region ICache Members

        /// <summary>
        ///   Gets the clock used by the cache.
        /// </summary>
        /// <value>The clock used by the cache.</value>
        /// <remarks>
        ///   This property belongs to the services which can be injected using the cache
        ///   constructor. If not specified, it defaults to <see cref="SystemClock"/>.
        /// </remarks>
        public sealed override IClock Clock { get; }

        /// <summary>
        ///   Gets the compressor used by the cache.
        /// </summary>
        /// <value>The compressor used by the cache.</value>
        /// <remarks>
        ///   This property belongs to the services which can be injected using the cache
        ///   constructor. If not specified, it defaults to <see cref="DeflateCompressor"/>.
        /// </remarks>
        public sealed override ICompressor Compressor { get; }

        /// <summary>
        ///   Gets the log used by the cache.
        /// </summary>
        /// <value>The log used by the cache.</value>
        /// <remarks>
        ///   This property belongs to the services which can be injected using the cache
        ///   constructor. If not specified, it defaults to what
        ///   <see cref="LogManager.GetLogger(System.Type)"/> returns.
        /// </remarks>
        public sealed override ILog Log { get; }

        /// <summary>
        ///   The maximum number of parent keys each item can have. SQLite based caches support up to
        ///   five parent keys per item.
        /// </summary>
        public sealed override int MaxParentKeyCountPerItem { get; } = 5;

        /// <summary>
        ///   The pool used to retrieve <see cref="MemoryStream"/> instances.
        /// </summary>
        /// <remarks>
        ///   This property belongs to the services which can be injected using the cache
        ///   constructor. If not specified, it defaults to <see cref="CodeProject.ObjectPool.Specialized.MemoryStreamPool.Instance"/>.
        /// </remarks>
        public sealed override IMemoryStreamPool MemoryStreamPool { get; }

        /// <summary>
        ///   Gets the serializer used by the cache.
        /// </summary>
        /// <value>The serializer used by the cache.</value>
        /// <remarks>
        ///   This property belongs to the services which can be injected using the cache
        ///   constructor. If not specified, it defaults to <see cref="JsonSerializer"/>. Therefore,
        ///   if you do not specify another serializer, make sure that your objects are serializable
        ///   (in most cases, simply use the <see cref="SerializableAttribute"/> and expose fields as
        ///   public properties).
        /// </remarks>
        public sealed override ISerializer Serializer { get; }

        /// <summary>
        ///   The available settings for the cache.
        /// </summary>
        public sealed override TSettings Settings { get; }

        /// <summary>
        ///   <c>true</c> if the Peek methods are implemented, <c>false</c> otherwise.
        /// </summary>
        public override bool CanPeek => true;

        #endregion ICache Members

        #region Private Methods

        /// <summary>
        ///   Adds given value with the specified expiry time and refresh internal.
        /// </summary>
        /// <typeparam name="TVal">The type of the value.</typeparam>
        /// <param name="partition">The partition.</param>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <param name="utcExpiry">The UTC expiry time.</param>
        /// <param name="interval">The refresh interval.</param>
        /// <param name="parentKeys">
        ///   Keys, belonging to current partition, on which the new item will depend.
        /// </param>
        protected sealed override void AddInternal<TVal>(string partition, string key, TVal value, DateTime utcExpiry, TimeSpan interval, IList<string> parentKeys)
        {
            // Compute all parameters _before_ opening the connection.
            var hash = TruncateAndHash(ref partition, ref key);

            // Serializing may be pretty expensive, therefore we keep it out of the connection.
            byte[] serializedValue;
            try
            {
                using (var memoryStream = MemoryStreamPool.GetObject().MemoryStream)
                {
                    using (var compressionStream = Compressor.CreateCompressionStream(memoryStream))
                    {
                        Serializer.SerializeToStream(value, compressionStream);
                    }
                    serializedValue = memoryStream.ToArray();
                }
            }
            catch (Exception ex)
            {
                LastError = ex;
                Log.ErrorFormat(ErrorMessages.InternalErrorOnSerializationFormat, ex, value.SafeToString());
                throw new ArgumentException(ErrorMessages.NotSerializableValue, ex);
            }

            var dbCacheItem = new DbCacheItem
            {
                Hash = hash,
                Partition = partition,
                Key = key,
                UtcCreation = Clock.UnixTime,
                UtcExpiry = utcExpiry.ToUnixTime(),
                Interval = (long) interval.TotalSeconds,
                Compressed = true,
                Value = serializedValue,
            };

            // Also add the parent keys, if any.
            string parentKey;
            var parentKeyCount = parentKeys?.Count ?? 0;
            if (parentKeyCount > 0)
            {
                parentKey = parentKeys[0];
                dbCacheItem.ParentHash0 = TruncateAndHash(ref partition, ref parentKey);
                dbCacheItem.ParentKey0 = parentKey;

                if (parentKeyCount > 1)
                {
                    parentKey = parentKeys[1];
                    dbCacheItem.ParentHash1 = TruncateAndHash(ref partition, ref parentKey);
                    dbCacheItem.ParentKey1 = parentKey;
                    
                    if (parentKeyCount > 2)
                    {
                        parentKey = parentKeys[2];
                        dbCacheItem.ParentHash2 = TruncateAndHash(ref partition, ref parentKey);
                        dbCacheItem.ParentKey2 = parentKey;

                        if (parentKeyCount > 3)
                        {
                            parentKey = parentKeys[3];
                            dbCacheItem.ParentHash3 = TruncateAndHash(ref partition, ref parentKey);
                            dbCacheItem.ParentKey3 = parentKey;

                            if (parentKeyCount > 4)
                            {
                                parentKey = parentKeys[4];
                                dbCacheItem.ParentHash4 = TruncateAndHash(ref partition, ref parentKey);
                                dbCacheItem.ParentKey4 = parentKey;
                            }
                        }
                    }
                }
            }

            using (var db = new DbCacheContext(ConnectionFactory))
            {
                var dbCacheItemEntry = db.Entry(dbCacheItem);
                dbCacheItemEntry.State = EntityState.Added;

                if (!TrySaveChanges(db))
                {
                    dbCacheItemEntry.State = EntityState.Modified;
                    TrySaveChanges(db);
                }
            }
        }

        /// <summary>
        ///   Clears this instance or a partition, if specified.
        /// </summary>
        /// <param name="partition">The optional partition.</param>
        /// <param name="cacheReadMode">The cache read mode.</param>
        /// <returns>The number of items that have been removed.</returns>
        protected sealed override long ClearInternal(string partition, CacheReadMode cacheReadMode)
        {
            // Compute all parameters _before_ opening the connection.
            partition = partition?.Truncate(ConnectionFactory.MaxPartitionNameLength);
            var ignoreExpiryDate = (cacheReadMode == CacheReadMode.IgnoreExpiryDate);
            var utcNow = Clock.UnixTime;

            using (var db = new DbCacheContext(ConnectionFactory))
            {
                var hashes = db.CacheItems
                    .Where(x => partition == null || x.Partition == partition)
                    .Where(x => ignoreExpiryDate || x.UtcExpiry < utcNow)
                    .Select(x => x.Hash)
                    .ToArray();
                
                try
                {
                    db.Configuration.AutoDetectChangesEnabled = false;

                    foreach (var hash in hashes)
                    {
                        db.Entry(new DbCacheItem { Hash = hash }).State = EntityState.Deleted;
                    }
                }
                finally
                {
                    db.Configuration.AutoDetectChangesEnabled = true;
                }

                TrySaveChanges(db);

                return hashes.LongLength;
            }
        }

        /// <summary>
        ///   Determines whether cache contains the specified partition and key.
        /// </summary>
        /// <param name="partition">The partition.</param>
        /// <param name="key">The key.</param>
        /// <returns>Whether cache contains the specified partition and key.</returns>
        /// <remarks>Calling this method does not extend sliding items lifetime.</remarks>
        protected sealed override bool ContainsInternal(string partition, string key)
        {
            // Compute all parameters _before_ opening the connection.
            var hash = TruncateAndHash(ref partition, ref key);
            var utcNow = Clock.UnixTime;

            using (var db = new DbCacheContext(ConnectionFactory))
            {
                // Search for at least one valid item.
                return db.CacheItems
                    .Where(x => x.Hash == hash)
                    .Any(x => x.UtcExpiry >= utcNow);
            }
        }

        /// <summary>
        ///   The number of items in the cache or in a partition, if specified.
        /// </summary>
        /// <param name="partition">The optional partition.</param>
        /// <param name="cacheReadMode">The cache read mode.</param>
        /// <returns>The number of items in the cache.</returns>
        /// <remarks>Calling this method does not extend sliding items lifetime.</remarks>
        protected sealed override long CountInternal(string partition, CacheReadMode cacheReadMode)
        {
            // Compute all parameters _before_ opening the connection.
            partition = partition?.Truncate(ConnectionFactory.MaxPartitionNameLength);
            var ignoreExpiryDate = (cacheReadMode == CacheReadMode.IgnoreExpiryDate);
            var utcNow = Clock.UnixTime;

            using (var db = new DbCacheContext(ConnectionFactory))
            {
                return db.CacheItems
                    .Where(x => partition == null || x.Partition == partition)
                    .Count(x => ignoreExpiryDate || x.UtcExpiry >= utcNow);
            }
        }

        /// <summary>
        ///   Gets the value with specified partition and key. If it is a "sliding" or "static"
        ///   value, its lifetime will be increased by the corresponding interval.
        /// </summary>
        /// <typeparam name="TVal">The type of the expected value.</typeparam>
        /// <param name="partition">The partition.</param>
        /// <param name="key">The key.</param>
        /// <returns>The value with specified partition and key.</returns>
        protected sealed override Option<TVal> GetInternal<TVal>(string partition, string key)
        {
            // Compute all parameters _before_ opening the connection.
            var hash = TruncateAndHash(ref partition, ref key);
            var utcNow = Clock.UnixTime;

            byte[] serializedValue;
            using (var db = new DbCacheContext(ConnectionFactory))
            {
                var dbCacheItem = db.CacheItems
                    .Where(x => x.Hash == hash)
                    .FirstOrDefault(x => x.UtcExpiry >= utcNow);

                if (dbCacheItem == null)
                {
                    // Nothing to deserialize, return None.
                    return Option.None<TVal>();
                }

                // Since we are in a "get" operation, we should also update the expiry.
                if (dbCacheItem.Interval > 0L)
                {
                    dbCacheItem.UtcExpiry = utcNow + dbCacheItem.Interval;
                    TrySaveChanges(db);
                }

                serializedValue = dbCacheItem.Value;
            }

            // Deserialize operation is expensive and it should be performed outside the connection.
            return DeserializeValue<TVal>(serializedValue, partition, key);
        }

        /// <summary>
        ///   Gets the cache item with specified partition and key. If it is a "sliding" or "static"
        ///   value, its lifetime will be increased by corresponding interval.
        /// </summary>
        /// <typeparam name="TVal">The type of the expected value.</typeparam>
        /// <param name="partition">The partition.</param>
        /// <param name="key">The key.</param>
        /// <returns>The cache item with specified partition and key.</returns>
        protected sealed override Option<ICacheItem<TVal>> GetItemInternal<TVal>(string partition, string key)
        {
            // Compute all parameters _before_ opening the connection.
            var hash = TruncateAndHash(ref partition, ref key);
            var utcNow = Clock.UnixTime;

            DbCacheItem dbCacheItem;
            using (var db = new DbCacheContext(ConnectionFactory))
            {
                dbCacheItem = db.CacheItems
                    .Where(x => x.Hash == hash)
                    .FirstOrDefault(x => x.UtcExpiry >= utcNow);

                if (dbCacheItem == null)
                {
                    // Nothing to deserialize, return None.
                    return Option.None<ICacheItem<TVal>>();
                }

                // Since we are in a "get" operation, we should also update the expiry.
                if (dbCacheItem.Interval > 0L)
                {
                    dbCacheItem.UtcExpiry = utcNow + dbCacheItem.Interval;
                    TrySaveChanges(db);
                }
            }

            // Deserialize operation is expensive and it should be performed outside the connection.
            return DeserializeCacheItem<TVal>(dbCacheItem);
        }

        /// <summary>
        ///   Gets all cache items or the ones in a partition, if specified. If an item is a
        ///   "sliding" or "static" value, its lifetime will be increased by corresponding interval.
        /// </summary>
        /// <param name="partition">The optional partition.</param>
        /// <typeparam name="TVal">The type of the expected values.</typeparam>
        /// <returns>All cache items.</returns>
        protected sealed override IList<ICacheItem<TVal>> GetItemsInternal<TVal>(string partition)
        {
            // Compute all parameters _before_ opening the connection.
            partition = partition?.Truncate(ConnectionFactory.MaxPartitionNameLength);
            var utcNow = Clock.UnixTime;

            DbCacheItem[] dbCacheItems;
            using (var db = new DbCacheContext(ConnectionFactory))
            {
                dbCacheItems = db.CacheItems
                    .Where(x => partition == null || x.Partition == partition)
                    .Where(x => x.UtcExpiry >= utcNow)
                    .ToArray();

                // Since we are in a "get" operation, we should also update the expiry.
                foreach (var dbCacheItem in dbCacheItems.Where(x => x.Interval > 0L))
                {
                    dbCacheItem.UtcExpiry = utcNow + dbCacheItem.Interval;
                }

                TrySaveChanges(db);
            }

            // Deserialize operation is expensive and it should be performed outside the connection.
            return dbCacheItems
                .Select(DeserializeCacheItem<TVal>)
                .Where(i => i.HasValue)
                .Select(i => i.Value)
                .ToArray();
        }

        /// <summary>
        ///   Gets the item corresponding to given partition and key, without updating expiry date.
        /// </summary>
        /// <typeparam name="TVal">The type of the expected values.</typeparam>
        /// <param name="partition">The partition.</param>
        /// <param name="key">The key.</param>
        /// <returns>
        ///   The item corresponding to given partition and key, without updating expiry date.
        /// </returns>
        protected sealed override Option<TVal> PeekInternal<TVal>(string partition, string key)
        {
            // Compute all parameters _before_ opening the connection.
            var hash = TruncateAndHash(ref partition, ref key);
            var utcNow = Clock.UnixTime;

            byte[] serializedValue;
            using (var db = new DbCacheContext(ConnectionFactory))
            {
                serializedValue = db.CacheItems
                    .Where(x => x.Hash == hash)
                    .Where(x => x.UtcExpiry >= utcNow)
                    .Select(x => x.Value)
                    .FirstOrDefault();

                if (serializedValue == null)
                {
                    // Nothing to deserialize, return None.
                    return Option.None<TVal>();
                }
            }

            // Deserialize operation is expensive and it should be performed outside the connection.
            return DeserializeValue<TVal>(serializedValue, partition, key);
        }

        /// <summary>
        ///   Gets the item corresponding to given partition and key, without updating expiry date.
        /// </summary>
        /// <typeparam name="TVal">The type of the expected values.</typeparam>
        /// <param name="partition">The partition.</param>
        /// <param name="key">The key.</param>
        /// <returns>
        ///   The item corresponding to given partition and key, without updating expiry date.
        /// </returns>
        protected sealed override Option<ICacheItem<TVal>> PeekItemInternal<TVal>(string partition, string key)
        {
            // Compute all parameters _before_ opening the connection.
            var hash = TruncateAndHash(ref partition, ref key);
            var utcNow = Clock.UnixTime;

            DbCacheItem dbCacheItem;
            using (var db = new DbCacheContext(ConnectionFactory))
            {
                dbCacheItem = db.CacheItems
                    .AsNoTracking()
                    .Where(x => x.Hash == hash)
                    .FirstOrDefault(x => x.UtcExpiry >= utcNow);

                if (dbCacheItem == null)
                {
                    // Nothing to deserialize, return None.
                    return Option.None<ICacheItem<TVal>>();
                }
            }

            // Deserialize operation is expensive and it should be performed outside the connection.
            return DeserializeCacheItem<TVal>(dbCacheItem);
        }

        /// <summary>
        ///   Gets the all values in the cache or in the specified partition, without updating expiry dates.
        /// </summary>
        /// <param name="partition">The optional partition.</param>
        /// <typeparam name="TVal">The type of the expected values.</typeparam>
        /// <returns>All values, without updating expiry dates.</returns>
        /// <remarks>
        ///   If you are uncertain of which type the value should have, you can always pass
        ///   <see cref="T:System.Object"/> as type parameter; that will work whether the required
        ///   value is a class or not.
        /// </remarks>
        protected sealed override IList<ICacheItem<TVal>> PeekItemsInternal<TVal>(string partition)
        {
            // Compute all parameters _before_ opening the connection.
            partition = partition?.Truncate(ConnectionFactory.MaxPartitionNameLength);
            var utcNow = Clock.UnixTime;

            DbCacheItem[] dbCacheItems;
            using (var db = new DbCacheContext(ConnectionFactory))
            {
                dbCacheItems = db.CacheItems
                    .AsNoTracking()
                    .Where(x => partition == null || x.Partition == partition)
                    .Where(x => x.UtcExpiry >= utcNow)
                    .ToArray();
            }

            // Deserialize operation is expensive and it should be performed outside the connection.
            return dbCacheItems
                .Select(DeserializeCacheItem<TVal>)
                .Where(i => i.HasValue)
                .Select(i => i.Value)
                .ToArray();
        }

        /// <summary>
        ///   Removes the value with given partition and key.
        /// </summary>
        /// <param name="partition">The partition.</param>
        /// <param name="key">The key.</param>
        protected sealed override void RemoveInternal(string partition, string key)
        {
            // Compute all parameters _before_ opening the connection.
            var hash = TruncateAndHash(ref partition, ref key);

            using (var db = new DbCacheContext(ConnectionFactory))
            {
                var dbCacheItem = db.CacheItems.FirstOrDefault(x => x.Hash == hash);
                if (dbCacheItem != null)
                {
                    db.CacheItems.Remove(dbCacheItem);
                    TrySaveChanges(db);
                }
            }
        }

        private TVal UnsafeDeserializeValue<TVal>(byte[] serializedValue)
        {
            using (var memoryStream = new MemoryStream(serializedValue))
            using (var decompressionStream = Compressor.CreateDecompressionStream(memoryStream))
            {
                return Serializer.DeserializeFromStream<TVal>(decompressionStream);
            }
        }

        private Option<TVal> DeserializeValue<TVal>(byte[] serializedValue, string partition, string key)
        {
            try
            {
                return Option.Some(UnsafeDeserializeValue<TVal>(serializedValue));
            }
            catch (Exception ex)
            {
                LastError = ex;

                // Something wrong happened during deserialization. Therefore, we remove the old
                // element (in order to avoid future errors) and we return None.
                RemoveInternal(partition, key);

                Log.Warn(ErrorMessages.InternalErrorOnDeserialization, ex);

                return Option.None<TVal>();
            }
        }

        private Option<ICacheItem<TVal>> DeserializeCacheItem<TVal>(DbCacheItem src)
        {
            try
            {
                var cacheItem = new CacheItem<TVal>
                {
                    Partition = src.Partition,
                    Key = src.Key,
                    Value = UnsafeDeserializeValue<TVal>(src.Value),
                    UtcCreation = DateTimeExtensions.UnixTimeStart.AddSeconds(src.UtcCreation),
                    UtcExpiry = DateTimeExtensions.UnixTimeStart.AddSeconds(src.UtcExpiry),
                    Interval = TimeSpan.FromSeconds(src.Interval)
                };

                // Quickly read the parent keys, if any.
                if (src.ParentKey0 != null)
                {
                    if (src.ParentKey1 != null)
                    {
                        if (src.ParentKey2 != null)
                        {
                            if (src.ParentKey3 != null)
                            {
                                if (src.ParentKey4 != null)
                                {
                                    cacheItem.ParentKeys = new[] { src.ParentKey0, src.ParentKey1, src.ParentKey2, src.ParentKey3, src.ParentKey4 };
                                }
                                else cacheItem.ParentKeys = new[] { src.ParentKey0, src.ParentKey1, src.ParentKey2, src.ParentKey3 };
                            }
                            else cacheItem.ParentKeys = new[] { src.ParentKey0, src.ParentKey1, src.ParentKey2 };
                        }
                        else cacheItem.ParentKeys = new[] { src.ParentKey0, src.ParentKey1 };
                    }
                    else cacheItem.ParentKeys = new[] { src.ParentKey0 };
                }
                else cacheItem.ParentKeys = CacheExtensions.NoParentKeys;

                return Option.Some<ICacheItem<TVal>>(cacheItem);
            }
            catch (Exception ex)
            {
                LastError = ex;

                // Something wrong happened during deserialization. Therefore, we remove the old
                // element (in order to avoid future errors) and we return None.
                RemoveInternal(src.Partition, src.Key);

                Log.Warn(ErrorMessages.InternalErrorOnDeserialization, ex);

                return Option.None<ICacheItem<TVal>>();
            }
        }

        private long TruncateAndHash(ref string partition, ref string key)
        {
            var cf = ConnectionFactory;

            partition = partition.Truncate(cf.MaxPartitionNameLength);
            key = key.Truncate(cf.MaxKeyNameLength);

            var ph = (long) XXHash.XXH32(Encoding.Default.GetBytes(partition));
            var kh = (long) XXHash.XXH32(Encoding.Default.GetBytes(key));

            return (ph << 32) + kh;
        }

        private bool TrySaveChanges(DbCacheContext db)
        {
            try
            {
                db.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                LastError = ex;
                return false;
            }
        }

        #endregion Private Methods
    }
}