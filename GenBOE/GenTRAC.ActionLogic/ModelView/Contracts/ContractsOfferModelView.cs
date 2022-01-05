// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2022 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.ActionLogic.ModelView
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using IES.Common;

    /// <summary>
    /// Contract Offers MV
    /// </summary>
    public class ContractsOfferModelView
    {
        /// <summary>
        /// Offer Id for an offer that will be ignored in the UI and then on save
        /// </summary>
        public static readonly int OFFER_TO_IGNORE_ID = -1000;

        /// <summary>
        /// Id
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Customer Offer Amount
        /// </summary>
        public int? CustomerOfferAmountInt { get; set; }

        /// <summary>
        /// LM Counter Offer Cost
        /// </summary>
        public int? LMCounterOfferCostInt { get; set; }

        /// <summary>
        /// LM Counter Offer COM
        /// </summary>
        public int? LMCounterOfferCOMInt { get; set; }

        /// <summary>
        /// LM Counter Offer Profit Fee
        /// </summary>
        public int? LMCounterOfferProfitFeeInt { get; set; }

        /// <summary>
        /// Customer Offer Date
        /// </summary>
        public DateTime? CustomerOfferDt { get; set; }

        /// <summary>
        /// LM Counter Offer Date
        /// </summary>
        public DateTime? LmCounterOfferDt { get; set; }

        /// <summary>
        /// Customer Offer Amount String
        /// </summary>
        [Display(Name = "Customer Offer Amount")]
        public string CustomerOfferAmount => this.CustomerOfferAmountInt?.ToString("C0");

        /// <summary>
        /// LM Counter Offer Cost
        /// </summary>
        [Display(Name = "LM Counter Offer Cost")]
        public string LMCounterOfferCost => this.LMCounterOfferCostInt?.ToString("C0");

        /// <summary>
        /// LM Counter Offer COM
        /// </summary>
        [Display(Name = "LM Counter Offer COM")]
        public string LMCounterOfferCOM => this.LMCounterOfferCOMInt?.ToString("C0");

        /// <summary>
        /// LM Counter Offer Profit Fee
        /// </summary>
        [Display(Name = "LM Counter Offer Profit/Fee")]
        public string LMCounterOfferProfitFee => this.LMCounterOfferProfitFeeInt?.ToString("C0");

        /// <summary>
        /// LM Counter Offer Total Price Int
        /// </summary>
        private decimal? lmCounterOfferTotalPriceInt => this.LMCounterOfferCostInt + this.LMCounterOfferCOMInt + this.LMCounterOfferProfitFeeInt;

        /// <summary>
        /// Calculated - LM Counter Offer Price
        /// </summary>
        [Display(Name = "LM Counter Offer Total Price")]
        public string LMCounterOfferTotalPrice => lmCounterOfferTotalPriceInt?.ToString("C0");

        /// <summary>
        /// Calculated - LM Counter Offer ROS
        /// </summary>
        [Display(Name = "LM Counter Offer ROS %")]
        public string LMCounterOfferROS => ((this.LMCounterOfferCOMInt + this.LMCounterOfferProfitFeeInt) / this.lmCounterOfferTotalPriceInt)?.ToString("P");

        /// <summary>
        /// Customer Offer Date String
        /// </summary>
        [Display(Name = "Customer Offer Date")]
        public string CustomerOfferDate
        {
            get => this.CustomerOfferDt?.Date.ToShortDateString();

            set
            {
                this.CustomerOfferDt = null;
                if (DateTime.TryParse(value, out DateTime result))
                {
                    this.CustomerOfferDt = result.Normalize(DateTimePrecision.Day);
                }
            }
        }

        /// <summary>
        /// LM Counter Offer Date String
        /// </summary>
        [Display(Name = "LM Counter Offer Date")]
        public string LMCounterOfferDate
        {
            get => this.LmCounterOfferDt?.Date.ToShortDateString();

            set
            {
                this.LmCounterOfferDt = null;
                if (DateTime.TryParse(value, out DateTime result))
                {
                    this.LmCounterOfferDt = result.Normalize(DateTimePrecision.Day);
                }
            }
        }

        /// <summary>
        /// Hidden string, if applicable
        /// </summary>
        public string Hidden => this.Id == OFFER_TO_IGNORE_ID ? "hidden = \"hidden\"" : string.Empty;
    }
}
