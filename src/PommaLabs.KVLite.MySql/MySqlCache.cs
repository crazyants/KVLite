﻿// File name: MySqlCache.cs
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

using MySql.Data.MySqlClient;
using NodaTime;
using PommaLabs.KVLite.Database;
using PommaLabs.KVLite.Extensibility;
using System.Diagnostics.Contracts;

namespace PommaLabs.KVLite.MySql
{
    /// <summary>
    ///   Cache backed by MySQL.
    /// </summary>
    public class MySqlCache : DbCache<MySqlCacheSettings, MySqlConnection>
    {
        #region Default Instance

        /// <summary>
        ///   Gets the default instance for this cache kind. Default instance is configured using
        ///   default cache settings.
        /// </summary>
        [Pure]
#pragma warning disable CC0022 // Should dispose object

        public static MySqlCache DefaultInstance { get; } = new MySqlCache(new MySqlCacheSettings());

#pragma warning restore CC0022 // Should dispose object

        #endregion Default Instance

        /// <summary>
        ///   Initializes a new instance of the <see cref="MySqlCache"/> class with given settings.
        /// </summary>
        /// <param name="settings">Cache settings.</param>
        /// <param name="serializer">The serializer.</param>
        /// <param name="compressor">The compressor.</param>
        /// <param name="clock">The clock.</param>
        public MySqlCache(MySqlCacheSettings settings, ISerializer serializer = null, ICompressor compressor = null, IClock clock = null)
            : this(settings, new MySqlCacheConnectionFactory(), serializer, compressor, clock)
        {
        }

        /// <summary>
        ///   Initializes a new instance of the <see cref="MySql"/> class with given settings and
        ///   specified connection factory.
        /// </summary>
        /// <param name="settings">Cache settings.</param>
        /// <param name="connectionFactory">Cache connection factory.</param>
        /// <param name="serializer">The serializer.</param>
        /// <param name="compressor">The compressor.</param>
        /// <param name="clock">The clock.</param>
        public MySqlCache(MySqlCacheSettings settings, MySqlCacheConnectionFactory connectionFactory, ISerializer serializer = null, ICompressor compressor = null, IClock clock = null)
            : base(settings, connectionFactory, serializer, compressor, clock)
        {
        }
    }
}
