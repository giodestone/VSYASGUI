using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace VSYASGUI_Mod
{
    /// <summary>
    /// Calculates the CPU usage as an async task.
    /// </summary>
    internal class CpuLoadCalc
    {
        private Process _TrackedProcess;
        private Config _Config;

        private CancellationTokenSource _CancellationTokenSource;

        /// <summary>
        /// The current CPU usage as a percentage (0 to 1).
        /// </summary>
        public double ProcessorUsagePercentage { get; private set; } = 0;

        /// <summary>
        /// Creates a new task that runs in the background measuing the CPU usage.
        /// </summary>
        /// <param name="processToTrack">The process of which the CPU usage should be tracked.</param>
        /// <param name="config">The configuration.</param>
        public CpuLoadCalc(Process processToTrack, Config config)
        {
            _TrackedProcess = processToTrack;
            _Config = config;
            
            _CancellationTokenSource = new CancellationTokenSource();
            _ = MeasureCPUUsage(_CancellationTokenSource.Token);
        }

        ~CpuLoadCalc()
        {
            try
            {
                _CancellationTokenSource.Cancel();
            }
            catch { }
        }

        /// <summary>
        /// Task that runs forever measuring the CPU every certain time, as defined by <see cref="Config.CPUUsagePollTimerMs"/>.
        /// </summary>
        private async Task MeasureCPUUsage(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                TimeSpan measuermentStartTime = _TrackedProcess.TotalProcessorTime;
                Stopwatch timer = Stopwatch.StartNew();

                await Task.Delay(_Config.CPUUsagePollTimerMs, cancellationToken);

                TimeSpan measuerementEndTime = _TrackedProcess.TotalProcessorTime;
                timer.Stop();

                ProcessorUsagePercentage = (measuerementEndTime - measuermentStartTime).TotalMicroseconds / (Environment.ProcessorCount * timer.ElapsedMilliseconds);
            }
        }
    }
}
