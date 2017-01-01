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

            Test(new BlockingPipeStream());
            Test(new BufferingPipeStream());
            Test(new CompatBufferingPipeStream());
            Test(new LinkedListPipeStream());
        }
    }
}
