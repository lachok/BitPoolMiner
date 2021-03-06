﻿using BitPoolMiner.Models;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Windows;
using System.IO;
using System.Text;
using System.Globalization;
using System.Linq;
using BitPoolMiner.Enums;
using BitPoolMiner.Utils;
using BitPoolMiner.Process;

namespace BitPoolMiner.Miners
{
    /// <summary>
    /// This class is for ccminer derived class.
    /// </summary>
    public class LyclMiner : Miner
    {
        public string MinerConfigArguments { get; private set; }
        public string MinerConfigFileName { get; private set; }

        public LyclMiner(HardwareType hardwareType, MinerBaseType minerBaseType, HardwareMonitor hardwareMonitor, bool is64Bit) : base("LyclMiner", hardwareType, minerBaseType, hardwareMonitor, is64Bit)
        {
            MinerWorkingDirectory = Path.Combine(Utils.Core.GetBaseMinersDir(), "LyclMiner015");
            MinerFileName = "lyclMiner.exe";

            ApiPort = 4028;
            HostName = "127.0.0.1";
        }

        public override void Start()
        {
            // Override the standard StartProcess since this miner requires a configuration file vs command line
            GenerateLyclConfig();

            MinerProcess = new BPMProcess();

            IsMining = true;
            MinerProcess.Start(MinerWorkingDirectory, MinerConfigArguments, MinerFileName, Hardware == HardwareType.AMD, MinerBaseType);
            MinerProcess.MinerProcess.Exited += MinerExited;
        }

        public override void Stop()
        {
            try
            {
                StopProcess();
            }
            catch (Exception e)
            {
                throw new ApplicationException(string.Format("There was an error killing the miner process {0} with PID {1}", MinerProcess.MinerProcess.ProcessName, MinerProcess.MinerProcess.Handle), e);
            }
        }

        #region LyclMiner config setup
        /// <summary>
        /// Sets the name/path of the LyclMiner config file
        /// </summary>
        private void SetupLyclConfigName()
        {
            MinerConfigFileName = $"lyclMiner{CoinType.ToString()}.conf";
            MinerConfigArguments = MinerConfigFileName; // no switches, just the config file name for now
        }

        /// <summary>
        /// Generates the new LyclConfig file
        /// </summary>
        private void GenerateLyclConfig()
        {
            SetupLyclConfigName();

        }
        #endregion

        #region Monitoring Statistics

        public async override void ReportStatsAsyc(Guid accountId, string workerName)
        {
            try
            {
                var minerMonitorStat = await GetRPCResponse(accountId, workerName);

                if (minerMonitorStat == null)
                    return;

                System.Threading.Thread.Sleep(4000);
                PostMinerMonitorStat(accountId, workerName, minerMonitorStat);
            }
            catch (Exception e)
            {
                NLogProcessing.LogError(e, "Error reporting stats for Ccminer");
            }
        }

        private async Task<MinerMonitorStat> GetRPCResponse(Guid accountId, string workerName)
        {
            MinerMonitorStat minerMonitorStat = new MinerMonitorStat();
            try
            {
                var clientSocket = new TcpClient();

                if (clientSocket.ConnectAsync(HostName, ApiPort).Wait(5000))
                {
                    //string get_menu_request = "{\"id\":1, \"method\":\"getstat\"}\n";
                    //string get_menu_request = "{\"id\":0,\"jsonrpc\":\"2.0\",\"method\":\"miner_getstat1\"}\n";
                    string get_menu_request = "summary";
                    NetworkStream serverStream = clientSocket.GetStream();
                    byte[] outStream = System.Text.Encoding.ASCII.GetBytes(get_menu_request);
                    serverStream.Write(outStream, 0, outStream.Length);
                    serverStream.Flush();

                    byte[] inStream = new byte[clientSocket.ReceiveBufferSize];
                    await serverStream.ReadAsync(inStream, 0, (int)clientSocket.ReceiveBufferSize);
                    string returndata = System.Text.Encoding.ASCII.GetString(inStream);
                    string[] jsonData = returndata.Split(',');

                    //ewbfTemplate = JsonConvert.DeserializeObject<EWBFTemplate>(jsonData);

                    // Close socket
                    clientSocket.Close();
                    clientSocket = null;
                }
                else
                {
                    NLogProcessing.LogInfo($"Could not connect to EWBF miner socket on port {ApiPort}");

                    // Return null object;
                    return null;
                }

                //var dictHW = PruvotApi.GetHwInfo(HostName, ApiPort);
                //var dictHist = PruvotApi.GetHistory(HostName, ApiPort);

                minerMonitorStat.AccountGuid = accountId;
                minerMonitorStat.WorkerName = workerName;
                minerMonitorStat.CoinType = CoinType;
                minerMonitorStat.MinerBaseType = MinerBaseType;

                var gpuList = new List<int>[0, 1, 2];
                    //(from element in dictHist
                    //           orderby element["GPU"] ascending
                    //           select element["GPU"]).Distinct();

                List<GPUMonitorStat> gpuMonitorStatList = new List<GPUMonitorStat>();

                foreach (var gpuNumber in gpuList)
                {
                    //var gpuHash = (from element in dictHist
                    //               orderby element["GPU"] ascending, element["TS"] descending
                    //               where element["GPU"] == gpuNumber
                    //               select element).FirstOrDefault();

                    //var gpuHw = (from hw in dictHW
                    //             where hw["GPU"] == gpuNumber
                    //             select hw).FirstOrDefault();

                    // Create new GPU monitor stats object and map values
                    GPUMonitorStat gpuMonitorStat = new GPUMonitorStat();

                    gpuMonitorStat.AccountGuid = accountId;
                    gpuMonitorStat.WorkerName = workerName;
                    gpuMonitorStat.CoinType = CoinType;
                    gpuMonitorStat.GPUID = Convert.ToInt32(gpuNumber);
                    //gpuMonitorStat.HashRate = (Convert.ToDecimal(gpuHash["KHS"]));
                    //gpuMonitorStat.FanSpeed = Convert.ToInt16(gpuHw["FAN"]);
                    //gpuMonitorStat.Temp = (short)Convert.ToDecimal(gpuHw["TEMP"]);
                    //gpuMonitorStat.Power = (short)(Convert.ToDecimal(gpuHw["POWER"]) / 1000);
                    gpuMonitorStat.HardwareType = Hardware;

                    // Sum up power and hashrate
                    minerMonitorStat.Power += (short)gpuMonitorStat.Power;
                    minerMonitorStat.HashRate += gpuMonitorStat.HashRate;

                    // Add GPU stats to list
                    gpuMonitorStatList.Add(gpuMonitorStat);

                }

                // Set list of GPU monitor stats
                minerMonitorStat.GPUMonitorStatList = gpuMonitorStatList;

                return await Task.Run(() => { return minerMonitorStat; });

            }
            catch (Exception ex)
            {
                NLogProcessing.LogError(ex, "Error calling GetRPCResponse from miner.");

                // Return null object;
                return null;
            }
        }


    }
    #endregion
}
