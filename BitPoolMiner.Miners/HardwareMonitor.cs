using System;
using System.Collections.ObjectModel;
using BitPoolMiner.Models;

namespace BitPoolMiner.Miners
{
    public interface HardwareMonitor
    {
        ObservableCollection<GPUSettings> ScanHardware(Guid accountId, string workerName);
    }
}
