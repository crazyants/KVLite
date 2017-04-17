﻿// File name: IEssentialCache.cs
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

using CodeProject.ObjectPool.Specialized;
using PommaLabs.KVLite.Extensibility;
using System;

namespace PommaLabs.KVLite
{
    /// <summary>
    ///   Settings shared between <see cref="ICache"/> and <see cref="IAsyncCache"/>.
    /// </summary>
    public interface IEssentialCache : IDisposable
    {
        /// <summary>
        ///   Returns <c>true</c> if all "Peek" methods are implemented, <c>false</c> otherwise.
        /// </summary>
        bool CanPeek { get; }

        /// <summary>
        ///   Gets the clock used by the cache.
        /// </summary>
        /// <remarks>
        ///   This property belongs to the services which can be injected using the cache
        ///   constructor. If not specified, it defaults to <see cref="SystemClock"/>.
        /// </remarks>
        IClock Clock { get; }

        /// <summary>
        ///   Gets the compressor used by the cache.
        /// </summary>
        /// <remarks>
        ///   This property belongs to the services which can be injected using the cache
        ///   constructor. Its default value depends on current implementation.
        /// </remarks>
        ICompressor Compressor { get; }

        /// <summary>
        ///   Whether this cache has been disposed or not. When a cache has been disposed, no more
        ///   operations are allowed on it.
        /// </summary>
        bool Disposed { get; }

        /// <summary>
        ///   The last error "swallowed" by the cache. All caches should try to swallow as much
        ///   exceptions as possible, because a failure in the cache should never harm the main
        ///   application. This is an important rule, which sometimes gets forgotten.
        ///
        ///   This property might be used to expose the last error occurred while processing cache
        ///   items. If no error has occurred, this property will simply be null.
        ///
        ///   Every error is carefully registered using an internal logger, so no information is lost
        ///   when the cache swallows the exception.
        /// </summary>
        Exception LastError { get; }

        /// <summary>
        ///   The maximum number of parent keys each item can have.
        /// </summary>
        int MaxParentKeyCountPerItem { get; }

        /// <summary>
        ///   The pool used to retrieve <see cref="System.IO.MemoryStream"/> instances.
        /// </summary>
        /// <remarks>
        ///   This property belongs to the services which can be injected using the cache
        ///   constructor. If not specified, it defaults to <see cref="MemoryStreamPool.Instance"/>.
        /// </remarks>
        IMemoryStreamPool MemoryStreamPool { get; }

        /// <summary>
        ///   Gets the serializer used by the cache.
        /// </summary>
        /// <remarks>
        ///   This property belongs to the services which can be injected using the cache
        ///   constructor. Its default value depends on current implementation..
        /// </remarks>
        ISerializer Serializer { get; }
    }
}
