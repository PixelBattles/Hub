using BenchmarkDotNet.Running;

namespace PixelBattles.Hub.Server.Benchmarks
{
    class Program
    {
        static void Main(string[] args) => BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
    }
}
