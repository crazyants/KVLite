﻿// File name: IDbCacheConnectionFactory.cs
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

using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace PommaLabs.KVLite.Core
{
    /// <summary>
    ///   Creates new connections to a specified SQL provider.
    /// </summary>
    public interface IDbCacheConnectionFactory
    {
        #region Configuration

        string CacheSchemaName { get; set; }

        string CacheItemsTableName { get; set; }

        string CacheValuesTableName { get; set; }

        int MaxPartitionNameLength { get; }

        int MaxKeyNameLength { get; }

        /// <summary>
        ///   The connection string used to connect to the cache data provider.
        /// </summary>
        string ConnectionString { get; set; }

        #endregion Configuration

        #region Commands

        string InsertOrUpdateCacheItemCommand { get; }

        string DeleteCacheItemCommand { get; }

        string DeleteCacheItemsCommand { get; }

        string UpdateCacheItemExpiryCommand { get; }

        #endregion Commands

        #region Queries

        string ContainsCacheItemQuery { get; }

        string CountCacheItemsQuery { get; }

        string PeekCacheItemsQuery { get; }

        string PeekCacheItemQuery { get; }

        string PeekCacheValueQuery { get; }

        /// <summary>
        ///   Returns current cache size in bytes.
        /// </summary>
        string GetCacheSizeInBytesQuery { get; }

        #endregion Queries

        /// <summary>
        ///   Opens a new connection to the specified data provider.
        /// </summary>
        /// <returns>An open connection.</returns>
        DbConnection Open();

        /// <summary>
        ///   Opens a new connection to the specified data provider.
        /// </summary>
        /// <param name="cancellationToken">The cancellation instruction.</param>
        /// <returns>An open connection.</returns>
        Task<DbConnection> OpenAsync(CancellationToken cancellationToken);
    }
}