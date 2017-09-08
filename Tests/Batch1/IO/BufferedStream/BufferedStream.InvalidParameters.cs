// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Bridge.Test.NUnit;
using System;
using System.IO;

namespace Bridge.ClientTest.IO
{
    [Category(Constants.MODULE_IO)]
    [TestFixture(TestNameFormat = "BufferedStream_InvalidParameters - {0}")]
    public class BufferedStream_InvalidParameters
    {
        [Test]
        public static void NullConstructor_Throws_ArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new BufferedStream(null));
        }

        [Test]
        public static void NegativeBufferSize_Throws_ArgumentOutOfRangeException()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new BufferedStream(new MemoryStream(), -1));
        }

        [Test]
        public static void ZeroBufferSize_Throws_ArgumentNullException()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new BufferedStream(new MemoryStream(), 0));
        }

        [Test]
        public static void UnderlyingStreamDisposed_Throws_ObjectDisposedException()
        {
            MemoryStream disposedStream = new MemoryStream();
            disposedStream.Dispose();
            Assert.Throws<Exception>(() => new BufferedStream(disposedStream));
        }

        [Test]
        public static void SetPositionToNegativeValue_Throws_ArgumentOutOfRangeException()
        {
            using (BufferedStream stream = new BufferedStream(new MemoryStream()))
            {
                Assert.Throws<ArgumentOutOfRangeException>(() => stream.Position = -1);
            }
        }

        [Test]
        public static void Read_Arguments()
        {
            using (BufferedStream stream = new BufferedStream(new MemoryStream()))
            {
                byte[] array = new byte[10];
                Assert.Throws<ArgumentNullException>(() => stream.Read(null, 1, 1));
                Assert.Throws<ArgumentOutOfRangeException>(() => stream.Read(array, -1, 1));
                Assert.Throws<ArgumentOutOfRangeException>(() => stream.Read(array, 1, -1));
                Assert.Throws<ArgumentException>(() => stream.Read(array, 9, 2));
            }
        }

        [Test]
        public static void Write_Arguments()
        {
            using (BufferedStream stream = new BufferedStream(new MemoryStream()))
            {
                byte[] array = new byte[10];
                Assert.Throws<ArgumentNullException>(() => stream.Write(null, 1, 1));
                Assert.Throws<ArgumentOutOfRangeException>(() => stream.Write(array, -1, 1));
                Assert.Throws<ArgumentOutOfRangeException>(() => stream.Write(array, 1, -1));
                Assert.Throws<ArgumentException>(() => stream.Write(array, 9, 2));
            }
        }

        [Test]
        public static void SetLength_NegativeValue()
        {
            using (MemoryStream underlying = new MemoryStream())
            using (BufferedStream stream = new BufferedStream(underlying))
            {
                Assert.Throws<ArgumentOutOfRangeException>(() => stream.SetLength(-1));
                stream.SetLength(1);
                Assert.True(1 == underlying.Length);
                Assert.True(1 == stream.Length);
            }
        }

        [Test]
        public static void ReadOnUnreadableStream_Throws_NotSupportedException()
        {
            using (WrappedMemoryStream underlying = new WrappedMemoryStream(false, true, true))
            using (BufferedStream stream = new BufferedStream(underlying))
            {
                Assert.Throws<NotSupportedException>(() => stream.Read(new byte[] { 1 }, 0, 1));
            }
        }

        [Test]
        public static void WriteOnUnwritableStream_Throws_NotSupportedException()
        {
            using (WrappedMemoryStream underlying = new WrappedMemoryStream(true, false, true))
            using (BufferedStream stream = new BufferedStream(underlying))
            {
                Assert.Throws<NotSupportedException>(() => stream.Write(new byte[] { 1 }, 0, 1));
            }
        }

        [Test]
        public static void SeekOnUnseekableStream_Throws_NotSupportedException()
        {
            using (WrappedMemoryStream underlying = new WrappedMemoryStream(true, true, false))
            using (BufferedStream stream = new BufferedStream(underlying))
            {
                Assert.Throws<NotSupportedException>(() => stream.Seek(0, new SeekOrigin()));
            }
        }

        [Test]
        public void CopyTo_InvalidArguments_Throws()
        {
            using (var s = new BufferedStream(new MemoryStream()))
            {
                // Null destination
                Assert.Throws<ArgumentNullException>(() => { s.CopyTo(null); });

                // Buffer size out-of-range
                Assert.Throws<ArgumentOutOfRangeException>(() => { s.CopyTo(new MemoryStream(), 0); });
                Assert.Throws<ArgumentOutOfRangeException>(() => { s.CopyTo(new MemoryStream(), -1); });

                // Copying to non-writable stream
                Assert.Throws<NotSupportedException>(() => { s.CopyTo(new WrappedMemoryStream(canRead: true, canWrite: false, canSeek: true)); });

                // Copying to a non-writable and non-readable stream
                Assert.Throws<Exception>(() => { s.CopyTo(new WrappedMemoryStream(canRead: false, canWrite: false, canSeek: false)); });

                // Copying after disposing the buffer stream
                s.Dispose();
                Assert.Throws<Exception>(() => { s.CopyTo(new MemoryStream()); });
            }

            // Copying after disposing the underlying stream
            using (var ms = new MemoryStream())
            using (var s = new BufferedStream(ms))
            {
                ms.Dispose();
                Assert.Throws<Exception>(() => { s.CopyTo(new MemoryStream()); });
            }

            // Copying from a non-readable source
            using (var s = new BufferedStream(new WrappedMemoryStream(canRead: false, canWrite: true, canSeek: true)))
            {
                Assert.Throws<NotSupportedException>(() => { s.CopyTo(new MemoryStream()); });
            }
        }
    }
}
