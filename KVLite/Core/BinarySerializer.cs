﻿// File name: BinarySerializer.cs
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

using System.IO;
using System.IO.Compression;
using System.Runtime.Serialization.Formatters;
using System.Runtime.Serialization.Formatters.Binary;
using CodeProject.ObjectPool;
using PommaLabs.KVLite.Annotations;
using PommaLabs.KVLite.Core.Snappy;

namespace PommaLabs.KVLite.Core
{
    internal static class BinarySerializer
    {
        private const int MinBufferSize = 1024; // 1 KB

        private static readonly ObjectPool<PooledObjectWrapper<BinaryFormatter>> FormatterPool = new ObjectPool<PooledObjectWrapper<BinaryFormatter>>(1, 10, CreatePooledBinaryFormatter);
        
        [NotNull]
        public static byte[] SerializeObject([NotNull] object obj)
        {
            using (var memoryStream = new MemoryStream(MinBufferSize))
            {
                using (var snappyStream = new SnappyStream(memoryStream, CompressionMode.Compress, true))
                using (var binaryFormatter = FormatterPool.GetObject())
                {
                    binaryFormatter.InternalResource.Serialize(snappyStream, obj);
                }
                // Leave this line _after_ previous using, so that we obtain automatic flushing.
                return memoryStream.ToArray();
            }
        }

        [NotNull]
        public static object DeserializeObject([NotNull] byte[] serialized)
        {
            using (var memoryStream = new MemoryStream(serialized, true))
            using (var snappyStream = new SnappyStream(memoryStream, CompressionMode.Decompress, true))
            using (var binaryFormatter = FormatterPool.GetObject())
            {
                return binaryFormatter.InternalResource.Deserialize(snappyStream);
            }
        }

        [NotNull]
        private static PooledObjectWrapper<BinaryFormatter> CreatePooledBinaryFormatter()
        {
            return new PooledObjectWrapper<BinaryFormatter>(new BinaryFormatter
            {
                // In simple mode, the assembly used during deserialization need not match exactly
                // the assembly used during serialization. Specifically, the version numbers need
                // not match as the LoadWithPartialName method is used to load the assembly.
                AssemblyFormat = FormatterAssemblyStyle.Simple,

                // The low deserialization level for .NET Framework remoting. It supports types
                // associated with basic remoting functionality.
                FilterLevel = TypeFilterLevel.Low,

                // Indicates that types can be stated only for arrays of objects, object members of
                // type Object, and ISerializable non-primitive value types. The XsdString and
                // TypesWhenNeeded settings are meant for high performance serialization between
                // services built on the same version of the .NET Framework. These two values do not
                // support VTS (Version Tolerant Serialization) because they intentionally omit type
                // information that VTS uses to skip or add optional fields and properties. You
                // should not use the XsdString or TypesWhenNeeded type formats when serializing and
                // deserializing types on a computer running a different version of the .NET
                // Framework than the computer on which the type was serialized. Serializing and
                // deserializing on computers running different versions of the .NET Framework
                // causes the formatter to skip serialization of type information, thus making it
                // impossible for the deserializer to skip optional fields if they are not present
                // in certain types that may exist in the other version of the .NET Framework. If
                // you must use XsdString or TypesWhenNeeded in such a scenario, you must provide
                // custom serialization for types that have changed from one version of the .NET
                // Framework to the other.
                TypeFormat = FormatterTypeStyle.TypesWhenNeeded,
            });
        }
    }
}