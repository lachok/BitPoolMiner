using BitPoolMiner.Models;
using OpenHardwareMonitor.Hardware;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using BitPoolMiner.Miners;

namespace BitPoolMiner.Utils.OpenHardwareMonitor
{
    public class WindowsHardwareMonitor : HardwareMonitor
    {
        public ObservableCollection<GPUSettings> ScanHardware(Guid accountId, string workerName)
        {
            ObservableCollection<GPUSettings> gpuSettingsList = new ObservableCollection<GPUSettings>();

            try
            {
                Computer myComputer = new Computer();
                myComputer.Open();
                myComputer.GPUEnabled = true;

                int amdCount = 0;
                int nvidiaCount = 0;

                foreach (var hardwareItem in myComputer.Hardware)
                {
                    if (hardwareItem.HardwareType == HardwareType.GpuNvidia || hardwareItem.HardwareType == HardwareType.GpuAti)
                    {
                        GPUSettings gpuSettings = new GPUSettings();

                        gpuSettings.AccountGuid = accountId;
                        gpuSettings.WorkerName = workerName;
                        gpuSettings.HardwareName = hardwareItem.Name;
                        gpuSettings.EnabledForMining = true;

                        try
                        {
                            gpuSettings.Fanspeed = Convert.ToInt16(hardwareItem.Sensors.Where(x => x.SensorType == SensorType.Control && x.Name == "GPU Fan").FirstOrDefault().Value);
                        }
                        catch
                        {
                            gpuSettings.Fanspeed = 0;
                        }

                        gpuSettings.EnabledForMining = true;

                        if (hardwareItem.HardwareType == HardwareType.GpuNvidia)
                        {
                            //gpuSettings.GPUID = Convert.ToUInt16(hardwareItem.Identifier.ToString().Replace("/nvidiagpu/", "").Replace("/atigpu/", "").Replace("}", ""));

                            gpuSettings.GPUID = nvidiaCount;
                            nvidiaCount++;

                            gpuSettings.HardwareType = Enums.HardwareType.Nvidia;
                            gpuSettings.CoinSelectedForMining = Enums.CoinType.HUSH;
                            gpuSettings.MinerBaseType = Enums.MinerBaseType.EWBF;

                        }
                        else if (hardwareItem.HardwareType == HardwareType.GpuAti)
                        {
                            //gpuSettings.GPUID = Convert.ToUInt16(hardwareItem.Identifier.ToString().Replace("/nvidiagpu/", "").Replace("/atigpu/", "").Replace("}", ""));

                            gpuSettings.GPUID = amdCount;
                            amdCount++;

                            gpuSettings.HardwareType = Enums.HardwareType.AMD;
                            gpuSettings.CoinSelectedForMining = Enums.CoinType.EXP;
                            gpuSettings.MinerBaseType = Enums.MinerBaseType.Claymore;
                        }

                        // Add GPU settings to list
                        gpuSettingsList.Add(gpuSettings);
                    }
                }

                return gpuSettingsList;
            }
            catch (Exception e)
            {
                throw new ApplicationException(string.Format("Error scanning hardware"), e);
            }
        }
    }
}
