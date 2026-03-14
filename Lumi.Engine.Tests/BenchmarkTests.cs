using Lumi.Bytecode;
using Lumi.VM;
using System.Diagnostics;

namespace Lumi.Engine.Tests;

[TestClass]
public sealed class BenchmarkTests
{
    public TestContext TestContext { get; set; }

    [TestMethod]
    public void Test_Simple_Binary_Operation()
    {
        // Arrange
        var source = "1 + 1";

        // Act + Benchmark
        const int iterations = 1000;

        Warmup(source);
        CollectBenchmarkData(iterations, source);
    }

    [TestMethod]
    public void Test_Simple_Chained_Binary_Operation()
    {
        // Arrange
        var source = "2 + 3 * 8 - 5";

        // Act + Benchmark
        const int iterations = 1000;

        Warmup(source);
        CollectBenchmarkData(iterations, source);
    }

    [TestMethod]
    public void Test_Variable_Declaration()
    {
        // Arrange
        var source = "let x -> 42;";

        // Act + Benchmark
        const int iterations = 1000;

        Warmup(source);
        CollectBenchmarkData(iterations, source);
    }

    [TestMethod]
    public void Test_Variable_Declaration_And_Printing()
    {
        // Arrange
        var source = "let x -> 42; print(x);";

        // Act + Benchmark
        const int iterations = 1000;

        Warmup(source);
        CollectBenchmarkData(iterations, source);
    }

    private static void ExecuteProgram(string source)
    {
        var parser = new Parser.Parser(source);
        var ast = parser.Parse();

        var bytecodeGenerator = new BytecodeGenerator();
        var bytecodeResult = bytecodeGenerator.Generate(ast);

        var vm = new VirtualMachine();
        vm.Execute(bytecodeResult);
    }

    private static void Warmup(string source)
    {
        // Warmup to reduce JIT and one-time costs
        for (int i = 0; i < 10; i++)
        {
            ExecuteProgram(source);
        }
    }

    private void CollectBenchmarkData(int iterations, string source)
    {
        // Make a best-effort to get a clean baseline
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        var proc = Process.GetCurrentProcess();
        var beforeProcMem = proc.PrivateMemorySize64;
        var beforeTotalMem = GC.GetTotalMemory(true);
        long beforeAllocated = 0;

        try
        {
            beforeAllocated = GC.GetAllocatedBytesForCurrentThread();
        }
        catch (MissingMethodException)
        {
            // Fallback for runtimes that don't support GetAllocatedBytesForCurrentThread
            beforeAllocated = 0;
        }

        var sw = Stopwatch.StartNew();
        for (int i = 0; i < iterations; i++)
        {
            ExecuteProgram(source);
        }
        sw.Stop();

        long afterAllocated = 0;
        try
        {
            afterAllocated = GC.GetAllocatedBytesForCurrentThread();
        }
        catch (MissingMethodException)
        {
            afterAllocated = 0;
        }

        var afterTotalMem = GC.GetTotalMemory(false);
        var afterProcMem = proc.PrivateMemorySize64;

        var allocatedBytes = (afterAllocated > 0 && beforeAllocated > 0) ? (afterAllocated - beforeAllocated) : -1;
        var totalMemDelta = afterTotalMem - beforeTotalMem;
        var procMemDelta = afterProcMem - beforeProcMem;

        // Assert / Report
        TestContext?.WriteLine($"Source: {source}");
        TestContext?.WriteLine($"Iterations: {iterations}");
        TestContext?.WriteLine($"Total time: {sw.Elapsed.TotalMilliseconds} ms");
        TestContext?.WriteLine($"Average time: {sw.Elapsed.TotalMilliseconds / iterations} ms");
        if (allocatedBytes >= 0)
            TestContext?.WriteLine($"Allocated bytes (thread): {allocatedBytes} bytes");
        else
            TestContext?.WriteLine("Allocated bytes (thread): not available on this runtime");
        TestContext?.WriteLine($"GC.GetTotalMemory delta: {totalMemDelta} bytes");
        TestContext?.WriteLine($"Process private memory delta: {procMemDelta} bytes");
    }
}
