﻿// File name: DbCacheEntry.cs
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

namespace PommaLabs.KVLite.Core
{
    /// <summary>
    ///   Represents a flat entry stored inside the cache.
    /// </summary>
    internal sealed class DbCacheEntry : DbCacheValue
    {
        /// <summary>
        ///   SQL column name of <see cref="Partition"/>.
        /// </summary>
        public const string PartitionColumn = "kvle_partition";

        /// <summary>
        ///   A partition holds a group of related keys.
        /// </summary>
        public string Partition { get; set; }

        /// <summary>
        ///   SQL column name of <see cref="Key"/>.
        /// </summary>
        public const string KeyColumn = "kvle_key";

        /// <summary>
        ///   A key uniquely identifies an entry inside a partition.
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        ///   SQL column name of <see cref="UtcCreation"/>.
        /// </summary>
        public const string UtcCreationColumn = "kvle_creation";

        /// <summary>
        ///   When the entry was created, expressed as seconds after UNIX epoch.
        /// </summary>
        public long UtcCreation { get; set; }

        /// <summary>
        ///   SQL column name of <see cref="ParentKey0"/>.
        /// </summary>
        public const string ParentKey0Column = "kvle_parent_key0";

        /// <summary>
        ///   Optional parent entry, used to link entries in a hierarchical way.
        /// </summary>
        public string ParentKey0 { get; set; }

        /// <summary>
        ///   SQL column name of <see cref="ParentKey1"/>.
        /// </summary>
        public const string ParentKey1Column = "kvle_parent_key1";

        /// <summary>
        ///   Optional parent entry, used to link entries in a hierarchical way.
        /// </summary>
        public string ParentKey1 { get; set; }

        /// <summary>
        ///   SQL column name of <see cref="ParentKey2"/>.
        /// </summary>
        public const string ParentKey2Column = "kvle_parent_key2";

        /// <summary>
        ///   Optional parent entry, used to link entries in a hierarchical way.
        /// </summary>
        public string ParentKey2 { get; set; }

        /// <summary>
        ///   SQL column name of <see cref="ParentKey3"/>.
        /// </summary>
        public const string ParentKey3Column = "kvle_parent_key3";

        /// <summary>
        ///   Optional parent entry, used to link entries in a hierarchical way.
        /// </summary>
        public string ParentKey3 { get; set; }

        /// <summary>
        ///   SQL column name of <see cref="ParentKey4"/>.
        /// </summary>
        public const string ParentKey4Column = "kvle_parent_key4";

        /// <summary>
        ///   Optional parent entry, used to link entries in a hierarchical way.
        /// </summary>
        public string ParentKey4 { get; set; }

        public sealed class Group
        {
            public string Partition { get; set; }

            public bool IgnoreExpiryDate { get; set; }

            public long UtcExpiry { get; set; }
        }

        public sealed class Single
        {
            public string Partition { get; set; }

            public string Key { get; set; }

            public bool IgnoreExpiryDate { get; set; }

            public long UtcExpiry { get; set; }
        }
    }
}