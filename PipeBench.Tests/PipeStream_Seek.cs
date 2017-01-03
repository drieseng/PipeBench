using Microsoft.VisualStudio.TestTools.UnitTesting;
using Renci.SshNet.Common;
using System;
using System.IO;

namespace PipeBench.Tests
{
    [TestClass]
    public class PipeStream_Seek
    {
        [TestMethod]
        public void SeekOriginBegin_OffsetNegative_PositionedAtBeginOfStream_ShouldThrowArgumentOutOfRangeException()
        {
            var data = new byte[] { 1, 4, 3, 7 };
            var pipeStream = new LinkedListPipeStream();
            pipeStream.Write(data, 0, data.Length);

            try
            {
                pipeStream.Seek(-1L, SeekOrigin.Begin);
                Assert.Fail();
            }
            catch (ArgumentOutOfRangeException ex)
            {
                Assert.IsNull(ex.InnerException);
                Assert.AreEqual(string.Format("Cannot be less than the current position.{0}Parameter name: {1}", Environment.NewLine, ex.ParamName), ex.Message);
                Assert.AreEqual("offset", ex.ParamName);
            }
        }

        [TestMethod]
        public void SeekOriginBegin_OffsetNegative_PositionedAtMiddleOfStream_ShouldThrowArgumentOutOfRangeException()
        {
            var data = new byte[] { 1, 4, 3, 7 };
            var pipeStream = new LinkedListPipeStream();
            pipeStream.Write(data, 0, data.Length);
            pipeStream.Read(data, 0, 2);

            try
            {
                pipeStream.Seek(-1L, SeekOrigin.Begin);
                Assert.Fail();
            }
            catch (ArgumentOutOfRangeException ex)
            {
                Assert.IsNull(ex.InnerException);
                Assert.AreEqual(string.Format("Cannot be less than the current position.{0}Parameter name: {1}", Environment.NewLine, ex.ParamName), ex.Message);
                Assert.AreEqual("offset", ex.ParamName);
            }
        }

        [TestMethod]
        public void SeekOriginBegin_OffsetNegative_PositionedAtEndOfStream_ShouldThrowArgumentOutOfRangeException()
        {
            var data = new byte[] { 1, 4, 3, 7 };
            var pipeStream = new LinkedListPipeStream();
            pipeStream.Write(data, 0, data.Length);
            pipeStream.Read(data, 0, 4);

            try
            {
                pipeStream.Seek(-1L, SeekOrigin.Begin);
                Assert.Fail();
            }
            catch (ArgumentOutOfRangeException ex)
            {
                Assert.IsNull(ex.InnerException);
                Assert.AreEqual(string.Format("Cannot be less than the current position.{0}Parameter name: {1}", Environment.NewLine, ex.ParamName), ex.Message);
                Assert.AreEqual("offset", ex.ParamName);
            }
        }

        [TestMethod]
        public void SeekOriginBegin_OffsetLessThanPosition_ShouldThrowArgumentOutOfRangeException()
        {
            var data = new byte[] { 1, 4, 3, 7 };
            var pipeStream = new LinkedListPipeStream();
            pipeStream.Write(data, 0, data.Length);
            pipeStream.Read(data, 0, 2);

            try
            {
                pipeStream.Seek(1L, SeekOrigin.Begin);
                Assert.Fail();
            }
            catch (ArgumentOutOfRangeException ex)
            {
                Assert.IsNull(ex.InnerException);
                Assert.AreEqual(string.Format("Cannot be less than the current position.{0}Parameter name: {1}", Environment.NewLine, ex.ParamName), ex.Message);
                Assert.AreEqual("offset", ex.ParamName);
            }
        }

        [TestMethod]
        public void SeekOriginBegin_OffsetGreaterThanLength_ShouldThrowArgumentOutOfRangeException()
        {
            var data = new byte[] { 1, 4, 3, 7 };
            var pipeStream = new LinkedListPipeStream();
            pipeStream.Write(data, 0, data.Length);

            try
            {
                pipeStream.Seek(data.Length + 1, SeekOrigin.Begin);
                Assert.Fail();
            }
            catch (ArgumentOutOfRangeException ex)
            {
                Assert.IsNull(ex.InnerException);
                Assert.AreEqual(string.Format("Cannot seek beyond the end of the stream.{0}Parameter name: {1}", Environment.NewLine, ex.ParamName), ex.Message);
                Assert.AreEqual("offset", ex.ParamName);
            }
        }

        [TestMethod]
        public void SeekOriginBegin_OffsetEqualsPosition_PositionedAtBeginOfStream_ShouldNotMovePosition()
        {
            var data = new byte[] { 1, 4, 3, 7 };
            var pipeStream = new LinkedListPipeStream();
            pipeStream.Write(data, 0, data.Length);

            var newPosition = pipeStream.Seek(0L, SeekOrigin.Begin);

            Assert.AreEqual(0L, newPosition);
            Assert.AreEqual(0L, pipeStream.Position);

            var buffer = new byte[2];
            var bytesRead = pipeStream.Read(buffer, 0, buffer.Length);
            Assert.AreEqual(2, bytesRead);
            Assert.AreEqual(1, buffer[0]);
            Assert.AreEqual(4, buffer[1]);
        }

        [TestMethod]
        public void SeekOriginBegin_OffsetEqualsPosition_PositionedInMiddleOfStream_ShouldNotMovePosition()
        {
            var data = new byte[] { 1, 4, 3, 7 };
            var pipeStream = new LinkedListPipeStream();
            pipeStream.Write(data, 0, data.Length);
            pipeStream.Read(data, 0, 2);

            var newPosition = pipeStream.Seek(0L, SeekOrigin.Begin);

            Assert.AreEqual(2L, newPosition);
            Assert.AreEqual(2L, pipeStream.Position);

            var buffer = new byte[2];
            var bytesRead = pipeStream.Read(buffer, 0, buffer.Length);
            Assert.AreEqual(2, bytesRead);
            Assert.AreEqual(3, buffer[0]);
            Assert.AreEqual(7, buffer[1]);
        }

        [TestMethod]
        public void SeekOriginBegin_OffsetEqualsPosition_PositionedAtEndOfStream_ShouldNotMovePosition()
        {
            var data = new byte[] { 1, 4, 3, 7 };
            var pipeStream = new LinkedListPipeStream();
            pipeStream.Write(data, 0, data.Length);
            pipeStream.Read(data, 0, 4);

            var newPosition = pipeStream.Seek(data.Length, SeekOrigin.Begin);

            Assert.AreEqual(4L, newPosition);
            Assert.AreEqual(4L, pipeStream.Position);

            // allow detect EOS
            pipeStream.Dispose();

            var buffer = new byte[2];
            var bytesRead = pipeStream.Read(buffer, 0, buffer.Length);
            Assert.AreEqual(0, bytesRead);
            Assert.AreEqual(0, buffer[0]);
            Assert.AreEqual(0, buffer[1]);
        }

        [TestMethod]
        public void SeekOriginBegin_OffsetGreaterThanPositionAndLessThanLength_PositionedAtBeginOfStream()
        {
            var data = new byte[] { 1, 4, 3, 7 };
            var pipeStream = new LinkedListPipeStream();
            pipeStream.Write(data, 0, data.Length);

            var newPosition = pipeStream.Seek(3L, SeekOrigin.Begin);

            Assert.AreEqual(3L, newPosition);
            Assert.AreEqual(3L, pipeStream.Position);

            var buffer = new byte[1];
            var bytesRead = pipeStream.Read(buffer, 0, buffer.Length);
            Assert.AreEqual(1, bytesRead);
            Assert.AreEqual(7, buffer[0]);
        }

        [TestMethod]
        public void SeekOriginBegin_OffsetGreaterThanPositionAndEqualToLength_PositionedAtBeginOfStream()
        {
            var data = new byte[] { 1, 4, 3, 7 };
            var pipeStream = new LinkedListPipeStream();
            pipeStream.Write(data, 0, data.Length);

            var newPosition = pipeStream.Seek(4L, SeekOrigin.Begin);

            Assert.AreEqual(4L, newPosition);
            Assert.AreEqual(4L, pipeStream.Position);

            // allow detect EOS
            pipeStream.Dispose();

            var buffer = new byte[2];
            var bytesRead = pipeStream.Read(buffer, 0, buffer.Length);
            Assert.AreEqual(0, bytesRead);
            Assert.AreEqual(0, buffer[0]);
            Assert.AreEqual(0, buffer[1]);
        }

        [TestMethod]
        public void SeekOriginBegin_OffsetGreaterThanPositionAndLessThanLength_PositionedInMiddleOfStream()
        {
            var data = new byte[] { 1, 4, 3, 7 };
            var pipeStream = new LinkedListPipeStream();
            pipeStream.Write(data, 0, data.Length);
            pipeStream.Read(data, 0, 2);

            var newPosition = pipeStream.Seek(3L, SeekOrigin.Begin);

            Assert.AreEqual(3L, newPosition);
            Assert.AreEqual(3L, pipeStream.Position);

            var buffer = new byte[1];
            var bytesRead = pipeStream.Read(buffer, 0, buffer.Length);
            Assert.AreEqual(1, bytesRead);
            Assert.AreEqual(7, buffer[0]);
        }

        [TestMethod]
        public void SeekOriginBegin_OffsetGreaterThanPositionAndEqualToLength_PositionedInMiddleOfStream()
        {
            var data = new byte[] { 1, 4, 3, 7 };
            var pipeStream = new LinkedListPipeStream();
            pipeStream.Write(data, 0, data.Length);
            pipeStream.Read(data, 0, 2);

            var newPosition = pipeStream.Seek(4L, SeekOrigin.Begin);

            Assert.AreEqual(4L, newPosition);
            Assert.AreEqual(4L, pipeStream.Position);

            // allow detect EOS
            pipeStream.Dispose();

            var buffer = new byte[2];
            var bytesRead = pipeStream.Read(buffer, 0, buffer.Length);
            Assert.AreEqual(0, bytesRead);
            Assert.AreEqual(0, buffer[0]);
            Assert.AreEqual(0, buffer[1]);
        }

        [TestMethod]
        public void SeekOriginCurrent_OffsetNegative_PositionedAtBeginOfStream_ShouldThrowArgumentOutOfRangeException()
        {
            var data = new byte[] { 1, 4, 3, 7 };
            var pipeStream = new LinkedListPipeStream();
            pipeStream.Write(data, 0, data.Length);

            try
            {
                pipeStream.Seek(-1L, SeekOrigin.Current);
                Assert.Fail();
            }
            catch (ArgumentOutOfRangeException ex)
            {
                Assert.IsNull(ex.InnerException);
                Assert.AreEqual(string.Format("Cannot move backward.{0}Parameter name: {1}", Environment.NewLine, ex.ParamName), ex.Message);
                Assert.AreEqual("offset", ex.ParamName);
            }
        }

        [TestMethod]
        public void SeekOriginCurrent_OffsetNegative_PositionedInMiddleOfStream_ShouldThrowArgumentOutOfRangeException()
        {
            var data = new byte[] { 1, 4, 3, 7 };
            var pipeStream = new LinkedListPipeStream();
            pipeStream.Write(data, 0, data.Length);
            pipeStream.Read(data, 0, 2);

            try
            {
                pipeStream.Seek(-1L, SeekOrigin.Current);
                Assert.Fail();
            }
            catch (ArgumentOutOfRangeException ex)
            {
                Assert.IsNull(ex.InnerException);
                Assert.AreEqual(string.Format("Cannot move backward.{0}Parameter name: {1}", Environment.NewLine, ex.ParamName), ex.Message);
                Assert.AreEqual("offset", ex.ParamName);
            }
        }

        [TestMethod]
        public void SeekOriginCurrent_OffsetNegative_PositionedAtEndOfStream_ShouldThrowArgumentOutOfRangeException()
        {
            var data = new byte[] { 1, 4, 3, 7 };
            var pipeStream = new LinkedListPipeStream();
            pipeStream.Write(data, 0, data.Length);
            pipeStream.Read(data, 0, 4);

            try
            {
                pipeStream.Seek(-1L, SeekOrigin.Current);
                Assert.Fail();
            }
            catch (ArgumentOutOfRangeException ex)
            {
                Assert.IsNull(ex.InnerException);
                Assert.AreEqual(string.Format("Cannot move backward.{0}Parameter name: {1}", Environment.NewLine, ex.ParamName), ex.Message);
                Assert.AreEqual("offset", ex.ParamName);
            }
        }

        [TestMethod]
        public void SeekOriginCurrent_OffsetZero_PositionedAtBeginOfStream_ShouldNotMovePosition()
        {
            var data = new byte[] { 1, 4, 3, 7 };
            var pipeStream = new LinkedListPipeStream();
            pipeStream.Write(data, 0, data.Length);

            var newPosition = pipeStream.Seek(0L, SeekOrigin.Current);

            Assert.AreEqual(0L, newPosition);
            Assert.AreEqual(0L, pipeStream.Position);

            var buffer = new byte[2];
            var bytesRead = pipeStream.Read(buffer, 0, buffer.Length);
            Assert.AreEqual(2, bytesRead);
            Assert.AreEqual(1, buffer[0]);
            Assert.AreEqual(4, buffer[1]);
        }

        [TestMethod]
        public void SeekOriginCurrent_OffsetZero_PositionedAtMiddleOfStream_ShouldNotMovePosition()
        {
            var data = new byte[] { 1, 4, 3, 7 };
            var pipeStream = new LinkedListPipeStream();
            pipeStream.Write(data, 0, data.Length);
            pipeStream.Read(data, 0, 2);

            var newPosition = pipeStream.Seek(0L, SeekOrigin.Current);

            Assert.AreEqual(2L, newPosition);
            Assert.AreEqual(2L, pipeStream.Position);

            var buffer = new byte[2];
            var bytesRead = pipeStream.Read(buffer, 0, buffer.Length);
            Assert.AreEqual(2, bytesRead);
            Assert.AreEqual(3, buffer[0]);
            Assert.AreEqual(7, buffer[1]);
        }

        [TestMethod]
        public void SeekOriginCurrent_OffsetZero_PositionedAtEndOfStream_ShouldNotMovePosition()
        {
            var data = new byte[] { 1, 4, 3, 7 };
            var pipeStream = new LinkedListPipeStream();
            pipeStream.Write(data, 0, data.Length);
            pipeStream.Read(data, 0, 4);

            pipeStream.Seek(0L, SeekOrigin.Current);

            // allow detect EOS
            pipeStream.Dispose();

            var buffer = new byte[2];
            var bytesRead = pipeStream.Read(buffer, 0, buffer.Length);
            Assert.AreEqual(0, bytesRead);
            Assert.AreEqual(0, buffer[0]);
            Assert.AreEqual(0, buffer[1]);
        }

        [TestMethod]
        public void SeekOriginCurrent_OffsetPlusPositionAtMiddleOfStream()
        {
            var data = new byte[] { 1, 4, 3, 7 };
            var pipeStream = new LinkedListPipeStream();
            pipeStream.Write(data, 0, data.Length);
            pipeStream.Read(data, 0, 1);

            var newPosition = pipeStream.Seek(2L, SeekOrigin.Current);

            Assert.AreEqual(3L, newPosition);
            Assert.AreEqual(3L, pipeStream.Position);

            var buffer = new byte[1];
            var bytesRead = pipeStream.Read(buffer, 0, buffer.Length);
            Assert.AreEqual(1, bytesRead);
            Assert.AreEqual(7, buffer[0]);
        }

        [TestMethod]
        public void SeekOriginCurrent_OffsetPlusPositionAtEndOfStream()
        {
            var data = new byte[] { 1, 4, 3, 7 };
            var pipeStream = new LinkedListPipeStream();
            pipeStream.Write(data, 0, data.Length);
            pipeStream.Read(data, 0, 2);

            var newPosition = pipeStream.Seek(data.Length - pipeStream.Position, SeekOrigin.Current);

            Assert.AreEqual(4, newPosition);
            Assert.AreEqual(4, pipeStream.Position);

            // allow detect EOS
            pipeStream.Dispose();

            var buffer = new byte[2];
            var bytesRead = pipeStream.Read(buffer, 0, buffer.Length);
            Assert.AreEqual(0, bytesRead);
            Assert.AreEqual(0, buffer[0]);
            Assert.AreEqual(0, buffer[1]);
        }

        [TestMethod]
        public void SeekOriginCurrent_OffsetPlusPositionBeyondEndOfStream_ShouldThrowArgumentOutOfRangeException()
        {
            var data = new byte[] { 1, 4, 3, 7 };
            var pipeStream = new LinkedListPipeStream();
            pipeStream.Write(data, 0, data.Length);
            pipeStream.Read(data, 0, 2);

            try
            {
                pipeStream.Seek(data.Length - pipeStream.Position + 1, SeekOrigin.Current);
                Assert.Fail();
            }
            catch (ArgumentOutOfRangeException ex)
            {
                Assert.IsNull(ex.InnerException);
                Assert.AreEqual(string.Format("Cannot seek beyond the end of the stream.{0}Parameter name: {1}", Environment.NewLine, ex.ParamName), ex.Message);
                Assert.AreEqual("offset", ex.ParamName);
            }
        }
    }
}
