﻿// File name: AbstractCacheSettings.cs
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
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using PommaLabs.KVLite.Annotations;

namespace PommaLabs.KVLite.Core
{
    /// <summary>
    ///   Base class for cache settings. Contains settings shared among different caches.
    /// </summary>
    [Serializable]
    public abstract class AbstractCacheSettings : INotifyPropertyChanged
    {
        #region Fields

        private string _defaultPartition;
        private int _insertionCountBeforeCleanup;
        private int _maxCacheSizeInMB;
        private int _maxJournalSizeInMB;
        private int _staticIntervalInDays;

        internal TimeSpan StaticInterval;

        #endregion Fields

        #region Settings

        /// <summary>
        ///   The partition used when none is specified.
        /// </summary>
        public string DefaultPartition
        {
            get
            {
                Contract.Ensures(!String.IsNullOrWhiteSpace(Contract.Result<string>()));
                return _defaultPartition;
            }
            set
            {
                Contract.Requires<ArgumentException>(!String.IsNullOrWhiteSpace(value));
                _defaultPartition = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        ///   How many days static values will last.
        /// </summary>
        public int StaticIntervalInDays
        {
            get
            {
                Contract.Ensures(Contract.Result<int>() > 0);
                return _staticIntervalInDays;
            }
            set
            {
                Contract.Requires<ArgumentOutOfRangeException>(value > 0);
                _staticIntervalInDays = value;
                StaticInterval = TimeSpan.FromDays(value);
                OnPropertyChanged();
            }
        }

        /// <summary>
        ///   Number of inserts before a cache cleanup is issued.
        /// </summary>
        public int InsertionCountBeforeAutoClean
        {
            get
            {
                Contract.Ensures(Contract.Result<int>() > 0);
                return _insertionCountBeforeCleanup;
            }
            set
            {
                Contract.Requires<ArgumentOutOfRangeException>(value > 0);
                _insertionCountBeforeCleanup = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        ///   Max size in megabytes for the cache.
        /// </summary>
        public int MaxCacheSizeInMB
        {
            get
            {
                Contract.Ensures(Contract.Result<int>() > 0);
                return _maxCacheSizeInMB;
            }
            set
            {
                Contract.Requires<ArgumentOutOfRangeException>(value > 0);
                _maxCacheSizeInMB = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        ///   Max size in megabytes for the SQLite journal log.
        /// </summary>
        public int MaxJournalSizeInMB
        {
            get
            {
                Contract.Ensures(Contract.Result<int>() > 0);
                return _maxJournalSizeInMB;
            }
            set
            {
                Contract.Requires<ArgumentOutOfRangeException>(value > 0);
                _maxJournalSizeInMB = value;
                OnPropertyChanged();
            }
        }

        #endregion Settings

        #region Abstract Settings

        /// <summary>
        ///   Gets the cache URI; used for logging.
        /// </summary>
        /// <value>The cache URI.</value>
        public abstract string CacheUri { get; }

        #endregion Abstract Settings

        #region INotifyPropertyChanged Members

        /// <summary>
        ///   Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        ///   Called when a property changed.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        [NotifyPropertyChangedInvocator]
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion INotifyPropertyChanged Members
    }
}