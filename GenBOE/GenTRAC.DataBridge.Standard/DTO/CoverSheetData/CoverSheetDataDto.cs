// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2022 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.DataBridge.DTO
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using IES.Standard;

    /// <summary>
    /// Cover Sheet Data DTO Class
    /// </summary>
    [Serializable]
    public class CoverSheetDataDto : IES.Standard.UpdateableDTO, IES.Standard.Interfaces.ICachableDTO
    {
        /// <summary>
        /// for ICachableDTO..
        /// </summary>
        /// <returns>Primary Key</returns>
        public int GetPrimaryKeyID()
        {
            return this.Id;
        }

        /// <summary>
        /// Cover Sheet dto class
        /// </summary>
        public CoverSheetDataDto() { }

        /// <summary>
        /// Gets or sets Certification Completed Date
        /// </summary>
        public bool? IsCCPDRequired { get; set; }

        /// <summary>
        /// Cost Through Com
        /// </summary>
        public long? CostThroughCom { get; set; }

        /// <summary>
        /// Profit Fee
        /// </summary>
        public long? ProfitFee { get; set; }

        /// <summary>
        /// LM Space Total Price / Submitted Value
        /// </summary>
        public long? LMSpaceTotalPrice { get; set; }

        /// <summary>
        /// Gets or sets selected contract action type
        /// </summary>
        public ContractActionType? ContractActionType { get; set; }

        /// <summary>
        /// Gets or sets the text for the "Other" Contract Action Type
        /// </summary>
        public string OtherContractActionType { get; set; }

        /// <summary>
        /// Contract Type Group / Contract Type
        /// </summary>
        public int ContractTypeGroup { get; set; }

        /// <summary>
        /// Gets or sets CoverSheetApprover
        /// </summary>
        public string CoverSheetApproverNtid { get; set; }

        /// <summary>
        /// Gets or sets the date when CoverSheetApprover signed.
        /// </summary>
        public DateTime? CoverSheetApproverSignedDate { get; set; }

        /// <summary>
        /// Customer Submittal Date string
        /// </summary>
        public DateTime? CustomerSubmittalDate { get; set; }

        /// <summary>
        /// Cage #
        /// </summary>
        public string CageCode { get; set; }

        /// <summary>
        /// Address based on Cage # (containts address 1, address 2, city, state, zip)
        /// </summary>
        public ICollection<string> OfferorAddress { get; set; }

        /// <summary>
        /// Gets or sets the contract leader/PoC.
        /// </summary>
        public string ContractsLead { get; set; }
    }
}