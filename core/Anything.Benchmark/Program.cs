using System.Runtime.InteropServices;
using Anything.Benchmark.Thumbnails;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnostics.Windows;
using BenchmarkDotNet.Running;

namespace Anything.Benchmark
{
    public static class Program
    {
        public static void Main()
        {
            var config = DefaultConfig.Instance;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                config = config.AddDiagnoser(new NativeMemoryProfiler());
            }

            BenchmarkRunner.Run(typeof(FFmpegRendererBenchmark).Assembly, config);
        }
    }
}
