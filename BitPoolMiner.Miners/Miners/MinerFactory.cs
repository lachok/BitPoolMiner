using BitPoolMiner.Enums;
using System;

namespace BitPoolMiner.Miners
{
    /// <summary>
    /// This is the miner factor to construct each miner for each mining session instance
    /// </summary>
    public class MinerFactory
    {
        private HardwareMonitor hardwareMonitor;

        public MinerFactory(HardwareMonitor hardwareMonitor)
        {
            this.hardwareMonitor = hardwareMonitor;
        }

        public Miner CreateMiner(MinerBaseType minerBaseType, HardwareType hardwareType, bool is64Bit = true)
        {
            switch (minerBaseType)
            {
                case MinerBaseType.CCMiner:
                    return new Ccminer(hardwareType, minerBaseType, hardwareMonitor, is64Bit);

                case MinerBaseType.CCMinerNanashi:
                    return new CCMinerForkNanashi(hardwareType, minerBaseType, hardwareMonitor);

                case MinerBaseType.EWBF:
                    return new EWBF(hardwareType, minerBaseType, hardwareMonitor);

                case MinerBaseType.EWBF_NO_ASIC:
                    return new EWBF_NO_ASIC(hardwareType, minerBaseType, hardwareMonitor);

                case MinerBaseType.DSTM:
                    return new DSTM(hardwareType, minerBaseType, hardwareMonitor);

                case MinerBaseType.Claymore:
                    return new Claymore(hardwareType, minerBaseType, hardwareMonitor);

                default:
                    throw new ApplicationException(string.Format("The miner base type {0} is not yet supported.", minerBaseType.ToString()));
            }
        }
    }
}
