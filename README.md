# ETL Parallelism Performance in C#

This project compares performance of different ETL (Extract, Transform, Load) techniques in C# using .NET. It includes:

- ✅ Sequential execution
- ✅ Task parallelism
- ✅ Data parallelism
- ✅ SIMD and ILP (Instruction-Level Parallelism)
- ✅ Pipeline parallelism
- ✅ Combined SIMD + Data Parallelism

## 📊 Output

A bar chart is generated using [ScottPlot](https://scottplot.net/) to visualize execution times.

## 🚀 How to Run

1. Open the project in Visual Studio
2. Restore NuGet packages (make sure ScottPlot is installed)
3. Run the project
4. Check the generated chart.png

## 🛠 Technologies

- C#
- .NET
- ScottPlot
- Parallel Programming
