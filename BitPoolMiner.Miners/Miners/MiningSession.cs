﻿using System;
using System.Collections.Generic;

namespace BitPoolMiner.Miners
{
    /// <summary>
    /// This MiningSession class handles each mining session for each miner.
    /// </summary>
    public class MiningSession
    {
        private readonly List<Miner> Miners = new List<Miner>();

        /// <summary>
        /// Adds a miner to the session
        /// </summary>
        /// <param name="m"></param>
        public void AddMiner(Miner m)
        {
            Miners.Add(m);
        }

        /// <summary>
        /// Removes all miners from the session
        /// </summary>
        public void RemoveAllMiners()
        {
            StopMiningSession();
            Miners.Clear();
        }

        /// <summary>
        /// Starts the mining session
        /// </summary>
        public void StartMiningSession()
        {
            foreach (Miner m in Miners)
            {
                m.Start();
            }
        }

        /// <summary>
        /// Stops all miners in this session
        /// </summary>
        public void StopMiningSession()
        {
            foreach (Miner m in Miners)
            {
                m.Stop();
            }
        }

        /// <summary>
        /// Post the mining stats to the API and website
        /// </summary>
        public void GetMinerStatsAsync(Guid accountId, string workerName)
        {
            foreach (Miner m in Miners)
            {
                m.ReportStatsAsyc(accountId, workerName);
            }
        }
    }
}
