using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VSYASGUI;

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

        public double ProcessorUsagePercentage { get; private set; } = 0;

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
