// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.DataBridge.DTO
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using IES.Common;

    /// <summary>
    /// Checklist Dto Class
    /// </summary>
    [Serializable]
    public class ProposalChecklistDto : IES.Common.UpdateableDTO, IES.Common.Interfaces.ICachableDTO
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public ProposalChecklistDto()
        {
            this.Id = -1;
            this.PPRResponses = new Collection<ChecklistResponseItem>();
            this.PARResponses = new Collection<ChecklistResponseItem>();
            this.UserSaveInfo = new Dictionary<ChecklistResponseType, Dictionary<ChecklistType, ProposalChecklistSaveInfo>>();
            this.UserSaveInfo.Add(ChecklistResponseType.Pricer, new Dictionary<ChecklistType, ProposalChecklistSaveInfo>());
            this.UserSaveInfo.Add(ChecklistResponseType.Peer, new Dictionary<ChecklistType, ProposalChecklistSaveInfo>());
        }

        /// <summary>
        /// for ICachableDTO..
        /// </summary>
        /// <returns>Primary Key</returns>
        public int GetPrimaryKeyID()
        {
            return this.Id;
        }

        /// <summary>
        /// Proposal ID
        /// </summary>
        public int ProposalID { get; set; }

        /// <summary>
        /// Proposal Submittal Date
        /// </summary>
        public DateTime? ProposalSubmittalDate { get; set; }

        /// <summary>
        /// Submitted Value
        /// </summary>
        public long? SubmittedValue { get; set; }

        /// <summary>
        /// Absolute Value
        /// </summary>
        public long? AbsoluteValue { get; set; }

        /// <summary>
        /// LM Labor Hrs
        /// </summary>
        public decimal? LMLaborHrs { get; set; }

        /// <summary>
        /// LM Labor Cost
        /// </summary>
        public long? LMLaborCost { get; set; }

        /// <summary>
        /// Subcontractor Cost
        /// </summary>
        public long? SubcontractorCost { get; set; }

        /// <summary>
        /// Material Cost
        /// </summary>
        public long? MaterialCost { get; set; }

        /// <summary>
        /// IWTA Cost
        /// </summary>
        public long? IWTACost { get; set; }

        /// <summary>
        /// Travel Cost
        /// </summary>
        public long? TravelCost { get; set; }

        /// <summary>
        /// Other Direct Costs
        /// </summary>
        public long? OtherDirectCosts { get; set; }

        /// <summary>
        /// Profit Fee + COM (being replaced by Profit Fee / COM, but required for historical data)
        /// </summary>
        public long? ProfitFeeCOM { get; set; }

        /// <summary>
        /// Profit Fee
        /// </summary>
        public long? ProfitFee { get; set; }

        /// <summary>
        /// COM
        /// </summary>
        public long? COM { get; set; }

        /// <summary>
        /// ROS Percentage
        /// </summary>
        public decimal? ROSPercentage { get; set; }

        /// <summary>
        /// Collection of PPR Checklist responses
        /// </summary>
        public ICollection<ChecklistResponseItem> PPRResponses { get; set; }

        /// <summary>
        /// Collection of PPR Checklist responses
        /// </summary>
        public ICollection<ChecklistResponseItem> PARResponses { get; set; }

        /// <summary>
        /// Response Type
        /// </summary>
        public ChecklistResponseType ResponseType { get; set; }

        /// <summary>
        /// Boolean flag used to turn on/off PAR checklist ProposalPageNumber validation
        /// </summary>
        public bool DeliverChecklistDFARS { get; set; }

        /// <summary>
        /// Pricer save info
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public Dictionary<ChecklistResponseType, Dictionary<ChecklistType, ProposalChecklistSaveInfo>> UserSaveInfo { get; set; }

        /// <summary>
        /// Whether this is a save or a submit
        /// </summary>
        public bool IsSubmit { get; set; }
    }
}
