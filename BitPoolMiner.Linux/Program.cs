using BitPoolMiner.Enums;
using BitPoolMiner.Miners;
using BitPoolMiner.Models;
using BitPoolMiner.Persistence.API;
using BitPoolMiner.Persistence.FileSystem;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using BitPoolMiner.Linux;

namespace BitPoolMiner.Console
{

    class MinerAccount
    {
        public string AccountId { get; set; }
        public List<AccountWallet> AccountWalletList { get; set; }
        public WorkerSettings WorkerSettings { get; set; }
        public ObservableCollection<GPUSettings> GPUSettingsList { get; set; }
        public Region Region { get; set; }

        internal static MinerAccount Init()
        {
            // Attempt to read the GUID from the config file
            AccountIdentityFile accountIdentityFile = new AccountIdentityFile();
            var accountIdentity = accountIdentityFile.ReadJsonFromFile();

            var accountId = accountIdentity.AccountGuid;
            // Validate if a GUID was actually read
            if (accountId == Guid.Empty)
            {
                throw new Exception("Account Identity could not be found - you need to configure this node with an account id.");
            }


            AccountWalletAPI accountWalletAPI = new AccountWalletAPI();
            GPUSettingsAPI gpuSettingsAPI = new GPUSettingsAPI();
            WorkerSettingsFile workerSettingsFile = new WorkerSettingsFile();
            var workerSettings = workerSettingsFile.ReadJsonFromFile();

            return new MinerAccount() {
                AccountId = accountId.ToString(),
                AccountWalletList = accountWalletAPI.GetAccountWalletList(accountId.ToString()),
                WorkerSettings = workerSettings,
                GPUSettingsList = gpuSettingsAPI.GetGPUSettings(accountId.ToString(), workerSettings.WorkerName)
            };
            
        }
    }

    class Program
    {
        private const string BANNER =
            @"
__________________________            ___________  _______                    
___  __ )__(_)_  /___  __ \______________  /__   |/  /__(_)___________________
__  __  |_  /_  __/_  /_/ /  __ \  __ \_  /__  /|_/ /__  /__  __ \  _ \_  ___/
_  /_/ /_  / / /_ _  ____// /_/ / /_/ /  / _  /  / / _  / _  / / /  __/  /    
/_____/ /_/  \__/ /_/     \____/\____//_/  /_/  /_/  /_/  /_/ /_/\___//_/     
                                                                              ";
        static void Main(string[] args)
        {
            System.Console.WriteLine(BANNER);
            var minerAccount = MinerAccount.Init();
            var miningSession = new MiningSession();

            miningSession.RemoveAllMiners();
            System.Console.WriteLine($@"Account Id: {minerAccount.AccountId}");
            System.Console.WriteLine($@"Region: {minerAccount.Region}");
            System.Console.WriteLine($@"Worker: {minerAccount.WorkerSettings.WorkerName}");
            System.Console.WriteLine(@"Configured Wallets:");
            minerAccount.AccountWalletList.Select(w => $@"  {w.CoinName}: {w.WalletAddress}")
                .ToList().ForEach(System.Console.WriteLine);

            // Call API and retrieve a list of miner configurations used to start mining
            List<MinerConfigResponse> minerConfigResponseList = GetMinerConfigurations(minerAccount);
            var hardwareMonitor = new LinuxHardwareMonitor();
            var minerFactory = new MinerFactory(hardwareMonitor);
            // Iterate through returned responses from API and initialize miners
            foreach (MinerConfigResponse minerConfigResponse in minerConfigResponseList)
            {
                // Create miner session
                var miner = minerFactory.CreateMiner(minerConfigResponse.MinerBaseType, minerConfigResponse.HardwareType);
                miner.CoinType = minerConfigResponse.CoinSelectedForMining;
                miner.MinerArguments = minerConfigResponse.MinerConfigString;
                miningSession.AddMiner(miner);
                System.Console.WriteLine(string.Format("Mining started {0} {1}", minerConfigResponse.MinerBaseType, minerConfigResponse.MinerConfigString));
            }

            System.Console.ReadLine();
        }


        /// <summary>
        /// Call API and retrieve a list of miner configurations used to start mining
        /// </summary>
        /// <returns></returns>
        private static List<MinerConfigResponse> GetMinerConfigurations(MinerAccount minerAccount)
        {
            // Build api Request object
            MinerConfigRequest minerConfigRequest = new MinerConfigRequest();
            minerConfigRequest.Region = minerAccount.Region;
            minerConfigRequest.AccountWalletList = minerAccount.AccountWalletList;
            minerConfigRequest.GPUSettingsList = minerAccount.GPUSettingsList.ToList();

            // Call the web API the get a response with a list of miner config strings used
            // to start one or more mining sessions based on the current miners configurations
            MinerConfigStringAPI minerConfigStringAPI = new MinerConfigStringAPI();
            List<MinerConfigResponse> minerConfigResponseList = minerConfigStringAPI.GetMinerConfigResponses(minerConfigRequest);
            return minerConfigResponseList;
        }

    }
}
