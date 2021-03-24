using BenchmarkDotNet.Running;

namespace OwnHub.Benchmarks.Sqlite
{
    class Program
    {
        static void Main(string[] args)
        {
            BenchmarkRunner.Run<TriplesBenchmark>();
        }
    }
}
