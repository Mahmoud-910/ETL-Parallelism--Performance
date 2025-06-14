using ScottPlot;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;

class ETLParallelism
{
    const int DataSize = 10_000_000;
    static int[] rawData = new int[DataSize];

    static void Main()
    {
        Console.WriteLine("Initializing raw data...");
        Random rand = new Random();

        for (int i = 0; i < DataSize; i++)
            rawData[i] = rand.Next(1, 100);
        Console.WriteLine("Data initialization complete.");

        Console.WriteLine("\nStarting measurements...");
        var results = new List<(string Label, double Time)>
        {
            ("Sequential", Measure(SequentialETL, "Sequential")),
            ("Task Parallel (Refactored)", Measure(TaskParallelETL_Refactored, "Task Parallel (Refactored)")),
            ("Data Parallel (Refactored)", Measure(DataParallelETL_Refactored, "Data Parallel (Refactored)")),
            ("Pipeline Parallel", Measure(PipelineParallelETL, "Pipeline Parallel")),
            ("SIMD + ILP (Refactored)", Measure(SIMD_ILP_ETL_Refactored, "SIMD + ILP (Refactored)")),
            ("Combined (SIMD + Data Parallel)", Measure(CombinedETL, "Combined (SIMD + Data Parallel)"))
        };

        Console.WriteLine("\nExecution Times:");
        results.Sort((a, b) => a.Time.CompareTo(b.Time)); // Sort 
        foreach (var r in results)
            Console.WriteLine($"{r.Label}: {r.Time:F2} ms");


        try
        {
            Console.WriteLine("\nGenerating chart...");
            var plt = new ScottPlot.Plot();
            double[] values = results.Select(r => r.Time).ToArray();
            string[] labels = results.Select(r => r.Label).ToArray();

            var barPlot = plt.Add.Bars(values);

            var ticks = Enumerable.Range(0, labels.Length).Select(i => new ScottPlot.Tick(i, labels[i])).ToArray();


            plt.Axes.Bottom.Label.Text = "Technique";
            plt.Axes.Bottom.TickGenerator = new ScottPlot.TickGenerators.NumericManual(ticks);
            plt.Axes.Bottom.MajorTickStyle.Length = 0;
            plt.Axes.Bottom.TickLabelStyle.Rotation = 45;


            plt.Axes.Left.Label.Text = "Time (ms)";


            plt.Axes.Margins(bottom: 0.15);

            plt.Title("ETL Performance Comparison");
            plt.SavePng("chart.png", 800, 600);
            Console.WriteLine("Chart saved as chart.png");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error generating chart: {ex.Message}");
            Console.WriteLine("Ensure ScottPlot is correctly installed or accessible.");
        }
    }

    static double Measure(Action method, string label)
    {
        Console.WriteLine($"Running {label}...");


        GC.Collect();
        GC.WaitForPendingFinalizers();
        Stopwatch sw = Stopwatch.StartNew();
        method();
        sw.Stop();
        Console.WriteLine($"{label} finished.");
        return sw.Elapsed.TotalMilliseconds;
    }

    // Sequential Implementation
    static void SequentialETL()
    {
        var extracted = new int[DataSize];
        Array.Copy(rawData, extracted, DataSize);
        var transformed = new int[DataSize];
        for (int i = 0; i < DataSize; i++)
        {
            transformed[i] = extracted[i] * 2;
        }
        var loaded = new List<int>(DataSize / 3);
        for (int i = 0; i < DataSize; i++)
        {
            if (transformed[i] % 3 == 0)
            {
                loaded.Add(transformed[i]);
            }
        }
        var loadedArray = loaded.ToArray();
    }

    // Task Parallelism
    static void TaskParallelETL_Refactored()
    {
        var extracted = new int[DataSize];
        Parallel.For(0, DataSize, i => extracted[i] = rawData[i]);
        var transformed = new int[DataSize];
        Parallel.For(0, DataSize, i =>
        {
            transformed[i] = extracted[i] * 2;
        });
        var loaded = new ConcurrentBag<int>();
        Parallel.For(0, DataSize, i =>
        {
            if (transformed[i] % 3 == 0)
            {
                loaded.Add(transformed[i]);
            }
        });
        var loadedArray = loaded.ToArray();
    }

    // Data Parallelism
    static void DataParallelETL_Refactored()
    {
        var extracted = new int[DataSize];
        Parallel.For(0, DataSize, i => extracted[i] = rawData[i]);
        var transformed = new int[DataSize];
        Parallel.For(0, DataSize, i =>
        {
            transformed[i] = extracted[i] * 2;
        });
        var loaded = new ConcurrentBag<int>();
        Parallel.For(0, DataSize, i =>
        {
            if (transformed[i] % 3 == 0)
            {
                loaded.Add(transformed[i]);
            }
        });
        var loadedArray = loaded.ToArray();
    }

    // Pipeline Parallelism using BlockingCollection
    static void PipelineParallelETL()
    {

        var buffer1 = new BlockingCollection<int>(10000);
        var buffer2 = new BlockingCollection<int>(10000);
        var loaded = new ConcurrentBag<int>();

        // Pipeline Stages

        // Stage 1: Extract
        var extractTask = Task.Run(() =>
        {
            try
            {
                for (int i = 0; i < DataSize; i++)
                {
                    buffer1.Add(rawData[i]);
                }
            }
            finally { buffer1.CompleteAdding(); }
        });

        // Stage 2: Transform
        var transformTask = Task.Run(() =>
        {
            try
            {

                foreach (var item in buffer1.GetConsumingEnumerable())
                {
                    buffer2.Add(item * 2);
                }
            }
            finally { buffer2.CompleteAdding(); }
        });

        // Stage 3: Load (Filter)
        var loadTask = Task.Run(() =>
        {

            foreach (var item in buffer2.GetConsumingEnumerable())
            {
                if (item % 3 == 0)
                {
                    loaded.Add(item);
                }
            }
        });

        // Wait for all stages to complete
        Task.WaitAll(extractTask, transformTask, loadTask);
        var loadedArray = loaded.ToArray();
    }

    // Refactored SIMD + ILP
    static void SIMD_ILP_ETL_Refactored()
    {
        int vectorSize = Vector<int>.Count;
        var transformed = new int[DataSize];
        for (int i = 0; i <= DataSize - vectorSize; i += vectorSize)
        {
            var vec = new Vector<int>(rawData, i);
            var transformedVec = vec * 2;
            transformedVec.CopyTo(transformed, i);
        }
        for (int i = DataSize - (DataSize % vectorSize); i < DataSize; i++)
        {
            transformed[i] = rawData[i] * 2;
        }
        var loadedArray = transformed.AsParallel().Where(x => x % 3 == 0).ToArray();
    }

    // Combined Approach: SIMD in Parallel Transform, Parallel Load
    static void CombinedETL()
    {
        int vectorSize = Vector<int>.Count;
        var transformed = new int[DataSize];
        Parallel.For(0, DataSize / vectorSize, i =>
        {
            var vec = new Vector<int>(rawData, i * vectorSize);
            var transformedVec = vec * 2;
            transformedVec.CopyTo(transformed, i * vectorSize);
        });
        int remainderStart = DataSize - (DataSize % vectorSize);
        for (int i = remainderStart; i < DataSize; i++)
        {
            transformed[i] = rawData[i] * 2;
        }
        var loadedArray = transformed.AsParallel().Where(x => x % 3 == 0).ToArray();
    }
}