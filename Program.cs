using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Renci.SshNet.Common;

namespace PipeBench
{
    class MainClass
    {
        static long iterations = 1000000;

        public static void Writer(object data)
        {
            var stream = (Stream)data;

            if (!stream.CanWrite)
                throw new InvalidOperationException();

            var buf = new byte[65536];

            for (int i = 0; i < iterations; i++)
            {
                stream.Write(buf, 0, buf.Length);
            }

            Console.WriteLine("{0} test wrote {1} bytes in {2} iterations.", stream.GetType().Name, buf.Length * iterations, iterations);
            stream.Dispose();
        }

        public static void Test(Stream stream)
        {
            var buf = new byte[4096];
            var sw = new Stopwatch();
            var t = new Thread(Writer);
            int read;
            long total = 0;

            Console.WriteLine("{0} test starting.", stream.GetType().Name);

            sw.Start();
            t.Start(stream);

            while ((read = stream.Read(buf, 0, buf.Length)) > 0)
                total += read;

            t.Join();
            sw.Stop();

            Console.WriteLine("{0} test read {1} bytes in {2} seconds.", stream.GetType().Name, total, sw.ElapsedMilliseconds / 1000f);
        }

        public static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                iterations = Int64.Parse(args[0]);
            }

            SeekBeginTest(new LinkedListPipeStream());
            SeekCurrentTest(new LinkedListPipeStream());
            SetLengthTest1(new LinkedListPipeStream());

            Test(new BlockingPipeStream());
            Test(new BufferingPipeStream());
            Test(new CompatBufferingPipeStream());
            Test(new LinkedListPipeStream());
        }

        private static void SetLengthTest1(Stream stream)
        {
            stream.Write(new byte[] { 0x05, 0x02, 0x04, 0x01, 0x00, 0x06 }, 1, 4);
            stream.Write(new byte[] { 0x03, 0x01, 0x08, 0x0e, 0x0c, 0x03 }, 2, 3);

            var buffer = new byte[1];
            stream.Read(buffer, 0, 1);

            stream.SetLength(3);

            if (stream.Length != 3)
            {
                Console.WriteLine("Failure");
                return;
            }

            if (stream.Position != 1)
            {
                Console.WriteLine("Failure");
                return;
            }

            stream.Dispose();
            buffer = new byte[6];
            var bytesRead = stream.Read(buffer, 0, 6);

            if (bytesRead != 2)
            {
                Console.WriteLine("Failure");
                return;
            }

            if (buffer[0] != 0x04)
            {
                Console.WriteLine("Failure");
                return;
            }
            if (buffer[1] != 0x01)
            {
                Console.WriteLine("Failure");
                return;
            }
            if (buffer[2] != 0)
            {
                Console.WriteLine("Failure");
                return;
            }
            if (buffer[3] != 0)
            {
                Console.WriteLine("Failure");
                return;
            }
            if (buffer[4] != 0)
            {
                Console.WriteLine("Failure");
                return;
            }
            if (buffer[5] != 0)
            {
                Console.WriteLine("Failure");
                return;
            }
        }

        private static void SeekCurrentTest(Stream stream)
        {
            stream.Write(new byte[] { 0x05, 0x02, 0x04, 0x01, 0x00, 0x06 }, 1, 4);
            stream.Write(new byte[] { 0x03, 0x01, 0x08, 0x0e, 0x0c, 0x03 }, 2, 3);

            var buffer = new byte[6];
            var bytesRead = stream.Read(buffer, 3, 2);
            if (bytesRead != 2)
            {
                Console.WriteLine("Failure");
                return;
            }

            if (buffer[0] != 0)
            {
                Console.WriteLine("Failure");
                return;
            }
            if (buffer[1] != 0)
            {
                Console.WriteLine("Failure");
                return;
            }
            if (buffer[2] != 0)
            {
                Console.WriteLine("Failure");
                return;
            }
            if (buffer[3] != 0x02)
            {
                Console.WriteLine("Failure");
                return;
            }
            if (buffer[4] != 0x04)
            {
                Console.WriteLine("Failure");
                return;
            }
            if (buffer[5] != 0)
            {
                Console.WriteLine("Failure");
                return;
            }

            var newPosition = stream.Seek(3, SeekOrigin.Current);
            Console.WriteLine("new position=" + newPosition);

            buffer = new byte[4];
            bytesRead = stream.Read(buffer, 0, 2);
            if (bytesRead != 2)
            {
                Console.WriteLine("Failure");
                return;
            }

            if (buffer[0] != 0x0e)
            {
                Console.WriteLine("Failure");
                return;
            }
            if (buffer[1] != 0x0c)
            {
                Console.WriteLine("Failure");
                return;
            }
            if (buffer[2] != 0)
            {
                Console.WriteLine("Failure");
                return;
            }
            if (buffer[3] != 0)
            {
                Console.WriteLine("Failure");
                return;
            }
        }

        private static void SeekBeginTest(Stream stream)
        {
            stream.Write(new byte[] { 0x05, 0x02, 0x04, 0x01, 0x00, 0x06 }, 1, 4);
            stream.Write(new byte[] { 0x03, 0x01, 0x08, 0x0e, 0x0c, 0x03 }, 2, 3);

            var buffer = new byte[6];
            var bytesRead = stream.Read(buffer, 3, 2);
            if (bytesRead != 2)
            {
                Console.WriteLine("Failure");
                return;
            }

            var newPosition = stream.Seek(3, SeekOrigin.Begin);
            if (newPosition != 3)
            {
                Console.WriteLine("Failure");
                return;
            }

            if (stream.Position != 3)
            {
                Console.WriteLine("Failure");
                return;
            }

            buffer = new byte[4];
            bytesRead = stream.Read(buffer, 0, 2);
            if (bytesRead != 2)
            {
                Console.WriteLine("Failure");
                return;
            }

            if (buffer[0] != 0x00)
            {
                Console.WriteLine("Failure");
                return;
            }
            if (buffer[1] != 0x08)
            {
                Console.WriteLine("Failure");
                return;
            }

            if (stream.Position != 5)
            {
                Console.WriteLine("Failure");
                return;
            }
        }
    }
}
