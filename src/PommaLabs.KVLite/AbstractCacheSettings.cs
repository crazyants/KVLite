﻿// File name: AbstractCacheSettings.cs
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

using NodaTime;
using PommaLabs.Thrower;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace PommaLabs.KVLite
{
    /// <summary>
    ///   Base class for cache settings. Contains settings shared among different caches.
    /// </summary>
    [Serializable, DataContract]
    public abstract partial class AbstractCacheSettings<TSettings> : ICacheSettings
        where TSettings : AbstractCacheSettings<TSettings>
    {
        private string _defaultPartition;
        private int _staticIntervalInDays;

        /// <summary>
        ///   The partition used when none is specified.
        /// </summary>
        [DataMember]
        public string DefaultPartition
        {
            get
            {
                var result = _defaultPartition;

                // Postconditions
                Debug.Assert(!string.IsNullOrWhiteSpace(result));
                return result;
            }
            set
            {
                // Preconditions
                Raise.ArgumentException.IfIsNullOrWhiteSpace(value, nameof(DefaultPartition));

                _defaultPartition = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        ///   How many days static values will last.
        /// </summary>
        [DataMember]
        public int StaticIntervalInDays
        {
            get
            {
                var result = _staticIntervalInDays;

                // Postconditions
                Debug.Assert(result > 0);
                return result;
            }
            set
            {
                // Preconditions
                Raise.ArgumentOutOfRangeException.If(value <= 0);

                _staticIntervalInDays = value;
                StaticInterval = Duration.FromDays(value);
                OnPropertyChanged();
            }
        }

        /// <summary>
        ///   How long static values will last. Computed from <see cref="StaticIntervalInDays"/>.
        /// </summary>
        [IgnoreDataMember]
        public Duration StaticInterval { get; private set; }

        #region Abstract Settings

        /// <summary>
        ///   Gets the cache URI; used for logging.
        /// </summary>
        /// <value>The cache URI.</value>
        [IgnoreDataMember]
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
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion INotifyPropertyChanged Members
    }
}