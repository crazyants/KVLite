﻿// File name: ErrorMessages.cs
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
// DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

namespace PommaLabs.KVLite.Core
{
    /// <summary>
    ///   Error messages used in KVLite.
    /// </summary>
    static class ErrorMessages
    {
        public const string InternalErrorOnClearAll = "An error occurred while clearing all cache partitions.";
        public const string InternalErrorOnClearPartition = "An error occurred while clearing cache partition '{0}'.";
        public const string InternalErrorOnCountAll = "An error occurred while counting items in all cache partitions.";
        public const string InternalErrorOnCountPartition = "An error occurred while counting items in cache partition '{0}'.";
        public const string InternalErrorOnReadAll = "An error occurred while reading items in all cache partitions.";
        public const string InternalErrorOnReadPartition = "An error occurred while reading items in cache partition '{0}'.";
        public const string InternalErrorOnRead = "An error occurred while reading item '{0}/{1}' from the cache.";
        public const string InternalErrorOnWrite = "An error occurred while writing item '{0}/{1}' into the cache.";
        public const string InternalErrorOnVacuum = "An error occurred while applying VACUUM on the SQLite cache.";

        public const string InvalidCacheName = "In-memory cache name can only contain alphanumeric characters, dots and underscores.";
        public const string InvalidCacheReadMode = "An invalid enum value was given for cache read mode.";

        public const string NotSerializableValue = @"Only serializable objects can be stored in the cache. Try putting the [Serializable] attribute on your class, if possible.";

        public const string NullOrEmptyCacheName = @"Cache name cannot be null or empty.";
        public const string NullOrEmptyCacheFile = @"Cache file cannot be null or empty.";
        public const string NullCache = @"Cache cannot be null, please specify one valid cache or use either PersistentCache or VolatileCache default instances.";
        public const string NullCacheResolver = @"Cache resolver function cannot be null, please specify one non-null function.";
        public const string NullKey = @"Key cannot be null, please specify one non-null string.";
        public const string NullPartition = @"Partition cannot be null, please specify one non-null string.";
        public const string NullSettings = @"Settings cannot be null, please specify valid settings or use the default instance.";
        public const string NullValue = @"Value cannot be null, please specify one non-null object.";
        public const string NullValueGetter = @"Value getter function cannot be null, please specify one non-null function.";

        public const string CacheHasBeenDisposed = @"Cache instance has been disposed, therefore no more operations are allowed on it.";
        public const string MemoryCacheDoesNotAllowPeeking = @".NET memory cache does not allow peeking items, therefore this method is not implemented.";
    }
}