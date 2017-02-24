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

using System.ComponentModel;

namespace PommaLabs.KVLite
{
    /// <summary>
    ///   Settings shared between <see cref="ICache"/> and <see cref="IAsyncCache"/>.
    /// </summary>
    public interface IEssentialCacheSettings : INotifyPropertyChanged
    {
        /// <summary>
        ///   Gets the cache URI; can be used for logging.
        /// </summary>
        /// <value>The cache URI.</value>
        string CacheUri { get; }

        /// <summary>
        ///   The partition used when none is specified.
        /// </summary>
        string DefaultPartition { get; set; }

        /// <summary>
        ///   How many days static values will last.
        /// </summary>
        int StaticIntervalInDays { get; set; }
    }
}