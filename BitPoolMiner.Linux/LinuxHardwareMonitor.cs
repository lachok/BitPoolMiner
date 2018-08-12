using System;
using System.Collections.ObjectModel;
using BitPoolMiner.Miners;
using BitPoolMiner.Models;

namespace BitPoolMiner.Linux
{
    class LinuxHardwareMonitor : HardwareMonitor
    {
        public ObservableCollection<GPUSettings> ScanHardware(Guid accountId, string workerName)
        {
            throw new NotImplementedException();
        }
    }
}
