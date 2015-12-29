﻿// File name: CacheProvider.cs
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

using System;
using System.Threading.Tasks;
using EntityFramework.Caching;

namespace PommaLabs.KVLite.EntityFramework
{
    public sealed class CacheProvider : ICacheProvider
    {
        #region Constants

        const string EfCachePartition = "KVLite.EntityFramework.CacheProvider";

        #endregion

        readonly ICache _cache;

        public bool Add(CacheKey cacheKey, object value, CachePolicy cachePolicy)
        {
            throw new NotImplementedException();
        }

        public long ClearCache() => _cache.Clear(EfCachePartition);

        public int Expire(CacheTag cacheTag)
        {
            throw new NotImplementedException();
        }

        public object Get(CacheKey cacheKey)
        {
            throw new NotImplementedException();
        }

        public object GetOrAdd(CacheKey cacheKey, Func<CacheKey, object> valueFactory, CachePolicy cachePolicy)
        {
            throw new NotImplementedException();
        }

        public Task<object> GetOrAddAsync(CacheKey cacheKey, Func<CacheKey, Task<object>> valueFactory, CachePolicy cachePolicy)
        {
            throw new NotImplementedException();
        }

        public object Remove(CacheKey cacheKey)
        {
            throw new NotImplementedException();
        }

        public bool Set(CacheKey cacheKey, object value, CachePolicy cachePolicy)
        {
            throw new NotImplementedException();
        }
    }
}
