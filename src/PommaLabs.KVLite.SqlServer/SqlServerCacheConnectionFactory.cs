﻿// File name: SqlServerCacheConnectionFactory.cs
//
// Author(s): Alessio Parma <alessio.parma@gmail.com>
//
// The MIT License (MIT)
//
// Copyright (c) 2014-2017 Alessio Parma <alessio.parma@gmail.com>
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

using PommaLabs.KVLite.Database;
using System.Data.SqlClient;

namespace PommaLabs.KVLite.SqlServer
{
    /// <summary>
    ///   Cache connection factory specialized for SQL Server.
    /// </summary>
    public class SqlServerCacheConnectionFactory : DbCacheConnectionFactory<SqlConnection>
    {
        /// <summary>
        ///   Cache connection factory specialized for SQL Server.
        /// </summary>
        public SqlServerCacheConnectionFactory()
            : base(SqlClientFactory.Instance, null, null)
        {
        }

        /// <summary>
        ///   Function used to estimate cache size.
        /// </summary>
        protected override string LengthSqlFunction { get; } = "len";

        /// <summary>
        ///   The symbol used to enclose an identifier (left side).
        /// </summary>
        protected override string LeftIdentifierEncloser { get; } = "[";

        /// <summary>
        ///   The symbol used to enclose an identifier (right side).
        /// </summary>
        protected override string RightIdentifierEncloser { get; } = "]";

        /// <summary>
        ///   This method is called when either the cache schema name or the cache entries table name
        ///   have been changed by the user.
        /// </summary>
        protected override void UpdateCommandsAndQueries()
        {
            base.UpdateCommandsAndQueries();

            var p = ParameterPrefix;
            var s = SqlSchemaWithDot;

            #region Commands

            InsertOrUpdateCacheEntryCommand = MinifyQuery($@"
                update {s}{CacheEntriesTableName}
                   set {DbCacheValue.UtcExpiryColumn} = {p}{nameof(DbCacheValue.UtcExpiry)},
                       {DbCacheValue.IntervalColumn} = {p}{nameof(DbCacheValue.Interval)},
                       {DbCacheValue.ValueColumn} = {p}{nameof(DbCacheValue.Value)},
                       {DbCacheValue.CompressedColumn} = {p}{nameof(DbCacheValue.Compressed)},
                       {DbCacheValue.UtcCreationColumn} = {p}{nameof(DbCacheValue.UtcCreation)},
                       {DbCacheEntry.ParentKey0Column} = {p}{nameof(DbCacheEntry.ParentKey0)},
                       {DbCacheEntry.ParentKey1Column} = {p}{nameof(DbCacheEntry.ParentKey1)},
                       {DbCacheEntry.ParentKey2Column} = {p}{nameof(DbCacheEntry.ParentKey2)},
                       {DbCacheEntry.ParentKey3Column} = {p}{nameof(DbCacheEntry.ParentKey3)},
                       {DbCacheEntry.ParentKey4Column} = {p}{nameof(DbCacheEntry.ParentKey4)}
                 where {DbCacheValue.HashColumn} = {p}{nameof(DbCacheValue.Hash)}

                if @@rowcount = 0
                begin
                    insert into {s}{CacheEntriesTableName} (
                        {DbCacheValue.HashColumn},
                        {DbCacheValue.PartitionColumn},
                        {DbCacheValue.KeyColumn},
                        {DbCacheValue.UtcExpiryColumn},
                        {DbCacheValue.IntervalColumn},
                        {DbCacheValue.ValueColumn},
                        {DbCacheValue.CompressedColumn},
                        {DbCacheValue.UtcCreationColumn},
                        {DbCacheEntry.ParentKey0Column},
                        {DbCacheEntry.ParentKey1Column},
                        {DbCacheEntry.ParentKey2Column},
                        {DbCacheEntry.ParentKey3Column},
                        {DbCacheEntry.ParentKey4Column}
                    )
                    values (
                        {p}{nameof(DbCacheValue.Hash)},
                        {p}{nameof(DbCacheValue.Partition)},
                        {p}{nameof(DbCacheValue.Key)},
                        {p}{nameof(DbCacheValue.UtcExpiry)},
                        {p}{nameof(DbCacheValue.Interval)},
                        {p}{nameof(DbCacheValue.Value)},
                        {p}{nameof(DbCacheValue.Compressed)},
                        {p}{nameof(DbCacheValue.UtcCreation)},
                        {p}{nameof(DbCacheEntry.ParentKey0)},
                        {p}{nameof(DbCacheEntry.ParentKey1)},
                        {p}{nameof(DbCacheEntry.ParentKey2)},
                        {p}{nameof(DbCacheEntry.ParentKey3)},
                        {p}{nameof(DbCacheEntry.ParentKey4)}
                    )
                end
            ");

            #endregion Commands
        }
    }
}
