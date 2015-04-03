﻿// File name: PersistentCache.cs
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

using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using Ninject;
using PommaLabs.KVLite.Core;
using PommaLabs.Testing;

namespace PommaLabs.KVLite
{
    /// <summary>
    ///   An SQLite-based persistent cache.
    /// </summary>
    public sealed class PersistentCache : CacheBase<PersistentCache, PersistentCacheSettings>
    {
        #region Fields

        private readonly IClockService _clock;

        #endregion

        #region Construction

        static PersistentCache()
        {
            InitSQLite();
        }

        /// <summary>
        ///   Initializes a new instance of the <see cref="PersistentCache"/> class with default settings.
        /// </summary>
        public PersistentCache()
            : base(new PersistentCacheSettings())
        {
        }

        /// <summary>
        ///   Initializes a new instance of the <see cref="PersistentCache"/> class with given settings.
        /// </summary>
        /// <param name="settings">Cache settings.</param>
        [Inject]
        public PersistentCache(PersistentCacheSettings settings, IClockService clock = null)
            : base(settings)
        {
            _clock = clock;
        }

        #endregion Construction

        #region CacheBase Members

        /// <summary>
        ///   Returns whether the changed property is the data source.
        /// </summary>
        /// <param name="changedPropertyName">Name of the changed property.</param>
        /// <returns>Whether the changed property is the data source.</returns>
        protected override bool DataSourceHasChanged(string changedPropertyName)
        {
            return changedPropertyName.ToLower().Equals("cachefile");
        }

        /// <summary>
        ///   Gets the data source, that is, the location of the SQLite store (it may be a file path
        ///   or a memory URI).
        /// </summary>
        /// <param name="journalMode">The journal mode.</param>
        /// <returns>The SQLite data source that will be used by the cache.</returns>
        protected override string GetDataSource(out SQLiteJournalModeEnum journalMode)
        {
            // Map cache path, since it may be an IIS relative path.
            var mappedPath = Settings.CacheFile.MapPath();

            // If the directory which should contain the cache does not exist, then we create it.
            // SQLite will take care of creating the DB itself.
            var cacheDir = Path.GetDirectoryName(mappedPath);
            if (cacheDir != null && !Directory.Exists(cacheDir))
            {
                Directory.CreateDirectory(cacheDir);
            }

            journalMode = SQLiteJournalModeEnum.Wal;
            return mappedPath;
        }

        /// <summary>
        ///   Returns all property (or field) values, along with their names, so that they can be
        ///   used to produce a meaningful <see cref="M:PommaLabs.FormattableObject.ToString"/>.
        /// </summary>
        /// <returns>
        ///   Returns all property (or field) values, along with their names, so that they can be
        ///   used to produce a meaningful <see cref="M:PommaLabs.FormattableObject.ToString"/>.
        /// </returns>
        protected override IEnumerable<GKeyValuePair<string, string>> GetFormattingMembers()
        {
            yield return GKeyValuePair.Create("CacheFile", Settings.CacheFile);
        }

        #endregion CacheBase Members
    }
}