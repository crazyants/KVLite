﻿// File name: BinarySerializer.cs
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
using System.IO;

namespace PommaLabs.KVLite.Core
{
    internal static class BinarySerializer
    {
        /// <summary>
        ///   Represents the object data type for cache entries.
        /// </summary>
        private enum DataTypes : byte
        {
            Object = 0,
            ByteArray = 1,
            String = 2
        }

        public static void Serialize<T>(ISerializer serializer, Stream output, T value)
        {
            var maybeBytes = value as byte[];
            if (maybeBytes != null)
            {
                output.WriteByte((byte) DataTypes.ByteArray);
                output.Write(maybeBytes, 0, maybeBytes.Length);
            }
            else
            {
                var maybeString = value as string;
                if (maybeString != null)
                {
                    output.WriteByte((byte) DataTypes.String);
#pragma warning disable CC0022 // Stream is disposed outside this method!
                    var sw = new StreamWriter(output);
#pragma warning restore CC0022 // Stream is disposed outside this method!
                    sw.Write(maybeString);
                    sw.Flush();
                }
                else
                {
                    output.WriteByte((byte) DataTypes.Object);
                    serializer.SerializeToStream(value, output);
                }
            }
        }

        public static T Deserialize<T>(ISerializer serializer, IMemoryStreamPool memoryStreamPool, Stream input)
        {
            var dataType = (DataTypes) input.ReadByte();
            switch (dataType)
            {
                case DataTypes.Object:
                    return serializer.DeserializeFromStream<T>(input);

                case DataTypes.String:
#pragma warning disable CC0022 // Stream is disposed outside this method!
                    var sr = new StreamReader(input);
#pragma warning restore CC0022 // Stream is disposed outside this method!
                    return (T) (object) sr.ReadToEnd();

                case DataTypes.ByteArray:
                    using (var ms = memoryStreamPool.GetObject().MemoryStream)
                    {
                        input.CopyTo(ms);
                        return (T) (object) ms.ToArray();
                    }

                default:
                    throw new InvalidDataException(ErrorMessages.InvalidDataType);
            }
        }
    }
}
