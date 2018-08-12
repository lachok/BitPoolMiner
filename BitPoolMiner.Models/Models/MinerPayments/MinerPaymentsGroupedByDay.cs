using BitPoolMiner.Enums;
using Newtonsoft.Json;
using System;
using System.Windows;

namespace BitPoolMiner.Models.MinerPayments
{
    /// <summary>
    /// Represents miner payment data summarized by day
    /// </summary>
    public class MinerPaymentsGroupedByDay
    {
        /// <summary>
        /// Amount of payment
        /// </summary>
        [JsonProperty("pa")]
        public Decimal PaymentAmount { get; set; }

        /// <summary>
        /// Date of payment
        /// </summary>
        [JsonIgnore]
        public DateTime PaymentDate
        {
            get
            {
                return new DateTime(PaymentDateTicks);
            }
        }
        
        /// <summary>
        /// Date of payment ticks
        /// </summary>
        [JsonProperty("pd")]
        public long PaymentDateTicks { get; set; }

        #region Display Properties

        /// <summary>
        /// Amount paid in Fiat
        /// </summary>
        public Decimal PaymentAmountFiat { get; set; }

        /// <summary>
        /// Exchange rate used to convert to Fiat
        /// </summary>
        public Decimal FiatExchangeRate { get; set; }

        /// <summary>
        /// Coin type for payment
        /// </summary>
        public CoinType CoinType { get; set; }

        /// <summary>
        /// Get the Coin Logo file location
        public string CoinLogo { get; set; }

        /// <summary>
        /// Display date without timestamp
        /// </summary>
        public string DisplayPaymentDate
        {
            get
            {
                return PaymentDate.ToShortDateString();
            }
        }

        /// <summary>
        /// Display formatted payment amount
        /// </summary>
        public string DisplayPaymentAmount
        {
            get
            {
                return Math.Round(PaymentAmount, 2).ToString();
            }
        }

        #endregion
    }
}
