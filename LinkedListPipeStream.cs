using System;
using System.IO;
using System.Threading;

namespace Renci.SshNet.Common
{
    internal class LinkedListPipeStream : Stream
    {
        private PipeEntry _first;
        private PipeEntry _last;
        private bool _isDisposed;
        private readonly object _lock;

        public LinkedListPipeStream()
        {
            _lock = new object();
        }

        public override void Flush()
        {
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (buffer == null)
                throw new ArgumentNullException("buffer");
            if (offset + count > buffer.Length)
                throw new ArgumentException("The sum of offset and count is greater than the buffer length.");
            if (offset < 0 || count < 0)
                throw new ArgumentOutOfRangeException("offset", "offset or count is negative.");

            lock (_lock)
            {
                var totalBytesRead = 0;

                while (count > 0)
                {
                    while (_first == null && !_isDisposed)
                        Monitor.Wait(_lock);

                    if (_first == null)
                    {
                        return totalBytesRead;
                    }

                    var bytesRead = _first.Read(buffer, offset, count);
                    if (_first.IsEmpty)
                    {
                        _first = _first.Next;
                    }

                    count -= bytesRead;
                    totalBytesRead += bytesRead;
                    offset += bytesRead;
                }

                return totalBytesRead;
            }
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (buffer == null)
                throw new ArgumentNullException("buffer");
            if (offset + count > buffer.Length)
                throw new ArgumentException("The sum of offset and count is greater than the buffer length.");
            if (offset < 0 || count < 0)
                throw new ArgumentOutOfRangeException("offset", "offset or count is negative.");
            if (_isDisposed)
                throw CreateObjectDisposedException();
            if (count == 0)
                return;

            lock (_lock)
            {
                var last = new PipeEntry(buffer, offset, count);
                if (_last == null)
                    _last = last;
                else
                {
                    _last = _last.Next = last;
                }

                if (_first == null)
                {
                    _first = _last;
                }

                Monitor.Pulse(_lock);
            }
        }

        public override bool CanRead
        {
            get { return true; }
        }

        public override bool CanSeek
        {
            get { return false; }
        }

        public override bool CanWrite
        {
            get { return true; }
        }

        public override long Length
        {
            get
            {
                throw new NotSupportedException();
            }
        }

        public override long Position { get; set; }

        /// <summary>
        /// Releases the unmanaged resources used by the Stream and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        /// <remarks>
        /// Disposing a <see cref="PipeStream"/> will interrupt blocking read and write operations.
        /// </remarks>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (!_isDisposed)
            {
                lock (_lock)
                {
                    _isDisposed = true;
                    Monitor.Pulse(_lock);
                }
            }
        }

        private ObjectDisposedException CreateObjectDisposedException()
        {
            return new ObjectDisposedException(GetType().FullName);
        }
    }

    internal class PipeEntry
    {
        private readonly byte[] _data;
        public int Position;
        public int Length;

        public PipeEntry(byte[] data, int offset, int count)
        {
            _data = data;
            Position = offset;
            Length = count;
        }

        public int Read(byte[] dst, int offset, int count)
        {
            var bytesToCopy = count;
            var bytesAvailable = Length - Position;

            if (count > bytesAvailable)
                bytesToCopy = bytesAvailable;

            Buffer.BlockCopy(_data, Position, dst, offset, bytesToCopy);
            Position += bytesToCopy;
            return bytesToCopy;
        }

        public bool IsEmpty
        {
            get { return Position == Length; }
        }

        public PipeEntry Next { get; set; }
    }
}
