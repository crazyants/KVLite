﻿// File name: CachingBootstrapper.cs
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

using Common.Logging;
using Nancy;
using Nancy.Bootstrapper;
using Nancy.TinyIoc;
using PommaLabs.KVLite.Properties;
using System;
using System.Collections.Generic;
using System.IO;

namespace PommaLabs.KVLite.Nancy
{
    [CLSCompliant(false)]
    public abstract class CachingBootstrapper : DefaultNancyBootstrapper
    {
        #region Fields

        private static readonly ICache Cache = Settings.Default.Nancy_CacheKind.ParseCacheKind();

        #endregion Fields

        protected override void ApplicationStartup(TinyIoCContainer container, IPipelines pipelines)
        {
            base.ApplicationStartup(container, pipelines);
            pipelines.BeforeRequest += CheckCache;
            pipelines.AfterRequest += SetCache;
        }

        /// <summary>
        ///   Check to see if we have a cache entry - if we do, see if it has expired or not, if it
        ///   hasn't then return it, otherwise return null.
        /// </summary>
        /// <param name="context">Current context.</param>
        /// <returns>Response or null.</returns>
        private static Response CheckCache(NancyContext context)
        {
            var cacheKey = context.GetRequestFingerprint();
            var cachedSummary = Cache.Get<ResponseSummary>(Settings.Default.Nancy_CachePartition, cacheKey);
            return (cachedSummary == null) ? null : cachedSummary.ToResponse();
        }

        /// <summary>
        ///   Adds the current response to the cache if required Only stores by Path and stores the
        ///   response in a KVLite cache.
        /// </summary>
        /// <param name="context">Current context.</param>
        private static void SetCache(NancyContext context)
        {
            if (context.Response.StatusCode != HttpStatusCode.OK)
            {
                return;
            }

            object cacheSecondsObject;
            if (!context.Items.TryGetValue(ContextExtensions.OutputCacheTimeKey, out cacheSecondsObject))
            {
                return;
            }

            int cacheSeconds;
            if (!int.TryParse(cacheSecondsObject.ToString(), out cacheSeconds))
            {
                return;
            }

            // Disable further caching, as it must explicitly enabled.
            context.Items.Remove(ContextExtensions.OutputCacheTimeKey);

            // The response we are going to cache. We put it here, so that it can be used as
            // recovery in the catch clause below.
            var responseToBeCached = context.Response;

            try
            {
                var cacheKey = context.GetRequestFingerprint();
                var cachedSummary = new ResponseSummary(responseToBeCached);
                Cache.AddTimedAsync(Settings.Default.Nancy_CachePartition, cacheKey, cachedSummary, DateTime.UtcNow.AddSeconds(cacheSeconds));
                context.Response = cachedSummary.ToResponse();
            }
            catch (Exception ex)
            {
                const string errMsg = "Something bad happened while caching :-(";
                LogManager.GetCurrentClassLogger().Error(errMsg, ex);
                // Sets the old response, hoping it will work...
                context.Response = responseToBeCached;
            }
        }

        [Serializable]
        private sealed class ResponseSummary
        {
            private readonly string _contentType;
            private readonly IDictionary<string, string> _headers;
            private readonly HttpStatusCode _statusCode;
            private readonly byte[] _contents;

            public ResponseSummary(Response response)
            {
                _contentType = response.ContentType;
                _headers = response.Headers;
                _statusCode = response.StatusCode;

                using (var memoryStream = new MemoryStream())
                {
                    response.Contents(memoryStream);
                    _contents = memoryStream.GetBuffer();
                }
            }

            public Response ToResponse()
            {
                return new Response
                {
                    ContentType = _contentType,
                    Headers = _headers,
                    StatusCode = _statusCode,
                    Contents = stream => stream.Write(_contents, 0, _contents.Length)
                };
            }
        }
    }
}