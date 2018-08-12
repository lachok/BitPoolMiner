using BitPoolMiner.Models.MinerPayments;
using BitPoolMiner.Persistence.API.Base;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace BitPoolMiner.Persistence.API
{
    /// <summary>
    /// API handler for miner historical payments
    /// </summary>
    public class MinerPaymentsAPI : APIBase
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Call API and GET list of historical payments
        /// </summary>
        /// <returns></returns>
        public List<MinerPaymentSummary> GetMinerPayments(string accountId)
        {
            string apiURL = APIConstants.APIURL + APIEndpoints.GetMinerPayments;

            try
            {
                NameValueCollection nameValueCollection = new NameValueCollection();
                nameValueCollection.Add("AccountGuid", accountId);

                List<MinerPaymentSummary> minerPayments = DownloadSerializedJSONData<List<MinerPaymentSummary>>(apiURL, nameValueCollection);

                return minerPayments;
            }
            catch (Exception e)
            {
                logger.Error(e, $"Error getting miner payments from {apiURL}");
                return new List<MinerPaymentSummary>();
            }
        }

    }
}
