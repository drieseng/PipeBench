using System;
using System.IO;
using System.Threading;

namespace Renci.SshNet.Common
{
    internal class LinkedListPipeStream : Stream
    {
        private readonly object _lock;
        private PipeEntry _first;
        private PipeEntry _last;
        private bool _isDisposed;
        private long _position;
        private long _length;

        /// <summary>
        /// Initializes a new <see cref="LinkedListPipeStream"/> instance.
        /// </summary>
        public LinkedListPipeStream()
        {
            _lock = new object();
        }

        /// <summary>
        /// Overrides the <see cref="Stream.Flush()"/> so that no action is performed.
        /// </summary>
        /// <remarks>
        /// Because any data written to a <see cref="LinkedListPipeStream"/> is written to a memory
        /// buffer, this method is redundant.
        /// </remarks>
        public override void Flush()
        {
        }
        
        public override long Seek(long offset, SeekOrigin origin)
        {
            if (_isDisposed)
                throw CreateObjectDisposedException();

            if (offset == 0)
                return _position;

            lock (_lock)
            {
                switch (origin)
                {
                    case SeekOrigin.Begin:
                        {
                            if (offset < _position)
                                throw new ArgumentOutOfRangeException("Cannot be less than the current position.", nameof(offset));
                            if (offset > _length)
                                throw new ArgumentOutOfRangeException("Cannot seek beyond the end of the stream.", nameof(offset));

                            var bytesToSkipForward = offset - _position;
                            InternalSeekForward(bytesToSkipForward);
                            _position += bytesToSkipForward;
                        }
                        break;
                    case SeekOrigin.Current:
                        {
                            if (offset < 0)
                                throw new ArgumentOutOfRangeException("Cannot move backward.", nameof(offset));
                            if (offset > (_length - _position))
                                throw new ArgumentOutOfRangeException("Cannot seek beyond the end of the stream.", nameof(offset));

                            InternalSeekForward(offset);
                            _position += offset;
                        }
                        break;
                    case SeekOrigin.End:
                        {
                            if (offset > 0)
                                throw new ArgumentOutOfRangeException("Cannot seek beyond the end of the stream.", nameof(offset));
                            if (_length + offset < _position)
                                throw new ArgumentOutOfRangeException("Cannot move backward.", nameof(offset));

                            var bytesToSkipForward = _length + offset - _position;
                            InternalSeekForward(bytesToSkipForward);
                            _position += bytesToSkipForward;
                        }
                        break;
                }

                return _position;
            }
        }

        private void InternalSeekForward(long offset)
        {
            while (offset > 0)
            {
                var remainingBytesInCurrentPipe = _first.Length - _first.Position;
                if (remainingBytesInCurrentPipe >= offset)
                {
                    _first.Position += (int)offset;
                    offset = 0;
                }
                else
                {
                    _first = _first.Next;
                    offset -= remainingBytesInCurrentPipe;
                }
            }
        }

        public override void SetLength(long value)
        {
            if (value < 0)
                throw new ArgumentOutOfRangeException("Cannot be negative.", nameof(value));
            if (_isDisposed)
                throw CreateObjectDisposedException();

            lock (_lock)
            {
                if (value < _position)
                    throw new ArgumentOutOfRangeException("Cannot be less that the current position.", nameof(value));

                if (value > _length)
                    throw new ArgumentOutOfRangeException("Cannot be greater than the current length.", nameof(value));

                var bytesToSkip = value - _position;
                while (bytesToSkip > 0)
                {
                    var remainingBytesInCurrentPipe = _first.Length - _first.Position;
                    if (remainingBytesInCurrentPipe > bytesToSkip)
                    {
                        _first.Length = _first.Position + (int)bytesToSkip;
                        _first.Next = null;
                        bytesToSkip = 0;
                    }
                    else
                    {
                        _first = _first.Next;
                        bytesToSkip -= remainingBytesInCurrentPipe;
                    }
                }

                _length = value;
            }
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
                    _position += bytesRead;
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

                _length += count;

                Monitor.Pulse(_lock);
            }
        }

        public override bool CanRead
        {
            get { return true; }
        }

        public override bool CanSeek
        {
            get { return !_isDisposed; }
        }

        public override bool CanWrite
        {
            get { return !_isDisposed; }
        }

        public override long Length
        {
            get
            {
                return _length;
            }
        }

        public override long Position
        {
            get
            {
                return _position;
            }
            set
            {
                Seek(value, SeekOrigin.Begin);
            }
        }

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
        private int _offset;
        public int Position;
        public int Length;

        public PipeEntry(byte[] data, int offset, int count)
        {
            _data = data;
            _offset = offset;
            Length = count;
        }

        public int Read(byte[] dst, int offset, int count)
        {
            var bytesToCopy = count;
            var bytesAvailable = Length - Position;

            if (count > bytesAvailable)
                bytesToCopy = bytesAvailable;

            Buffer.BlockCopy(_data, _offset + Position, dst, offset, bytesToCopy);
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
