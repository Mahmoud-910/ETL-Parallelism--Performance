ETL Parallelism Performance Comparison

This project compares the performance of various ETL (Extract, Transform, Load) techniques using different parallelism models implemented in C#. It was developed as part of a Computer Architecture course.

🧠 Parallelism Models Compared

Sequential

Task Parallelism (Refactored)

Data Parallelism (Refactored)

Pipeline Parallelism

SIMD + ILP (Refactored)

Combined (SIMD + Data Parallel)


⚙ Technologies Used

.NET 8.0

ScottPlot for performance chart visualization

System.Threading.Tasks and Parallel.For for concurrency


📊 Performance Benchmark (Execution Times)

Technique	Execution Time (ms)

Combined (SIMD + Data Parallel): 24.23 ms
SIMD + ILP (Refactored): 49.35 ms
Data Parallel (Refactored): 53.50 ms
Task Parallel (Refactored): 99.33 ms
Sequential: 192.44 ms
Pipeline Parallel: 7215.06 ms
📈 Chart Output



❗ Why Did Pipeline Parallelism Perform Poorly?

Although pipeline parallelism is a powerful parallelism model in many scenarios, its performance in this project was significantly worse than other techniques due to several key reasons:

Thread Blocking Between Stages: Each stage in the pipeline had to wait for the previous one to finish processing and push data forward, leading to delays and underutilization of threads.

Buffer Management Overhead: The added complexity of managing buffers between stages introduced latency that outweighed the benefits of pipelining in this case.

Low Per-Stage Workload: Each ETL stage in this simulation was relatively lightweight. Without heavy computation per stage, the overhead of coordinating between stages dominated the total runtime.

High Context Switching Cost: Frequent switching between pipeline stages caused performance degradation compared to data-parallel techniques which distribute work more efficiently.


> ⚠ Conclusion:
Pipeline parallelism is not always the best choice — especially for small, fast ETL tasks. Its performance heavily depends on the nature of the workload and how much computation is done per stage.



✅ What I Learned

How different parallelism models impact ETL performance.

SIMD and Data Parallel techniques often outperform traditional task-based or pipeline models in lightweight workloads.

Visualizing performance helps identify bottlenecks and inefficiencies clearly.


📎 Project Structure

Program.cs – Entry point with model selection logic

Models/ – Contains different ETL implementation classes

Benchmark/ – Manages timing and result aggregation


🚀 Run the Project

# Clone the repo
git clone https://github.com/Mahmoud-910/ETL-Parallelism--Performance.git
cd ETL-Parallelism--Performance

# Build and run
.

🔗 Repository

https://github.com/Mahmoud-910/ETL-Parallelism--Performance


---

Feel free to ⭐ the repo or open an issue if you have feedback!
