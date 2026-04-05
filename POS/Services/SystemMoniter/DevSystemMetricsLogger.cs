using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

public class DevSystemMetricsLogger : BackgroundService
{
    private readonly ILogger<DevSystemMetricsLogger> _logger;

    public DevSystemMetricsLogger(ILogger<DevSystemMetricsLogger> logger)
    {
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var process = Process.GetCurrentProcess();

        TimeSpan prevCpuTime = process.TotalProcessorTime;
        DateTime prevTime = DateTime.UtcNow;

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(10000, stoppingToken); // 10 seconds interval
            process.Refresh();

            // Memory usage
            double memoryMB = process.WorkingSet64 / (1024.0 * 1024.0);

            // CPU calculation
            TimeSpan currentCpuTime = process.TotalProcessorTime;
            DateTime currentTime = DateTime.UtcNow;

            double cpuUsedMs = (currentCpuTime - prevCpuTime).TotalMilliseconds;
            double totalMsPassed = (currentTime - prevTime).TotalMilliseconds;

            double cpuPercent =
                cpuUsedMs / (Environment.ProcessorCount * totalMsPassed) * 100;

            prevCpuTime = currentCpuTime;
            prevTime = currentTime;

            // ThreadPool stats
            ThreadPool.GetAvailableThreads(out int workerAvailable, out int ioAvailable);
            ThreadPool.GetMaxThreads(out int workerMax, out int ioMax);

            int workerUsed = workerMax - workerAvailable;

            // GC stats
            int gen0 = GC.CollectionCount(0);
            int gen1 = GC.CollectionCount(1);
            int gen2 = GC.CollectionCount(2);

            _logger.LogInformation(
                "DEV METRICS | CPU: {Cpu:0.00}% | RAM: {Memory:0.00} MB | Threads: {Threads}/{MaxThreads} | GC: {Gen0}/{Gen1}/{Gen2}",
                cpuPercent,
                memoryMB,
                workerUsed,
                workerMax,
                gen0,
                gen1,
                gen2);
        }
    }
}