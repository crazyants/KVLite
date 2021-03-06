﻿// File name: IEssentialCacheSettings.cs
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
using System;
using System.ComponentModel;

namespace PommaLabs.KVLite
{
    /// <summary>
    ///   Settings shared between <see cref="ICache"/> and <see cref="IAsyncCache"/>.
    /// </summary>
    public interface IEssentialCacheSettings : INotifyPropertyChanged
    {
        /// <summary>
        ///   The cache name, can be used for logging.
        /// </summary>
        /// <value>The cache name.</value>
        string CacheName { get; set; }

        /// <summary>
        ///   The partition used when none is specified.
        /// </summary>
        string DefaultPartition { get; set; }

        /// <summary>
        ///   When a serialized value is longer than specified length, then the cache will compress
        ///   it. If a serialized value length is less than or equal to the specified length, then
        ///   the cache will not compress it. Defaults to 4096 bytes.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">
        ///   <paramref name="value"/> is less than zero.
        /// </exception>
        long MinValueLengthForCompression { get; set; }

        /// <summary>
        ///   How long static values will last.
        /// </summary>
        Duration StaticInterval { get; set; }
    }
}
