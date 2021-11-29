// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.DataBridge.DTO
{
    using System;

    /// <summary>
    /// Contracts Offers DTO
    /// </summary>
    public class ContractsOffersDto : IES.Common.UpdateableDTO
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ContractsOffersDtomentDto"/> class.
        /// </summary>
        public ContractsOffersDto()
        {
            this.Id = -1;
        }

        /// <summary>
        /// Contracts Data ID int
        /// </summary>
        public int ContractsDataId { get; set; }

        /// <summary>
        /// Customer Offer Amount String
        /// </summary>
        public long CustomerOfferAmount { get; set; }

        /// <summary>
        /// Customer Offer Date String
        /// </summary>
        public DateTime? CustomerOfferDate { get; set; }

        /// <summary>
        /// LM Counter Offer Date String
        /// </summary>
        public DateTime? LMCounterOfferDate { get; set; }

        /// <summary>
        /// LM Counter Offer Cost
        /// </summary>
        public long LMCounterOfferCost { get; set; }

        /// <summary>
        /// LM Counter Offer COM
        /// </summary>
        public long LMCounterOfferCOM { get; set; }

        /// <summary>
        /// LM Counter Offer Profit Fee
        /// </summary>
        public long LMCounterOfferProfitFee { get; set; }
    }
}
