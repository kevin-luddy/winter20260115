// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace RPM.DataBridge.Models
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// The Proposal information, the property names are specifically short because of JSON serialization sizes.
    /// </summary>
    [Serializable]
    public class ProposalModelView
    {
        /// <summary>
        /// Gets or sets the Proposal Title
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the lead pricer
        /// </summary>
        public string Lead { get; set; }

        /// <summary>
        /// Gets or sets the estimated proposal value in millions
        /// </summary>
        public decimal? EVal { get; set; }

        /// <summary>
        /// Gets or sets the submitted proposal value in millions
        /// </summary>
        public decimal? SVal { get; set; }
       
        /// <summary>
        /// Gets or sets the Line of Business
        /// </summary>
        public string LOB { get; set; }
                
        /// <summary>
        /// Gets or sets the type of BOE tool.
        /// </summary>
        public string BoeTool { get; set; }
        
        /// <summary>
        /// Gets or sets the program area.
        /// </summary>
        public string PA { get; set; }
        
        /// <summary>
        /// Gets or sets the customer.
        /// </summary>
        public string Cust { get; set; }

        /// <summary>
        /// Gets or sets the customer type
        /// </summary>
        public string CusType { get; set; }

        /// <summary>
        /// Gets or sets the Proposal type.
        /// </summary>
        public string PropType { get; set; }
       
        /// <summary>
        /// Gets or sets the pricer type.
        /// </summary>
        public string PricerType { get; set; }
        
        /// <summary>
        /// Gets or sets the pricing tool.
        /// </summary>
        public string PricingTool { get; set; }
        
        /// <summary>
        /// Gets or sets the Additional Pricing Resource 1
        /// </summary>
        public string Price1 { get; set; }
        
        /// <summary>
        /// Gets or sets the Additional Pricing Resource 2
        /// </summary>
        public string Price2 { get; set; }
        
        /// <summary>
        /// Gets or sets the cost volume lead.
        /// </summary>
        public string Cost { get; set; }
                
        /// <summary>
        /// Gets or sets the type of the contract.
        /// </summary>
        public string ContType { get; set; }
        
        /// <summary>
        /// Gets or sets the reference/tracking number.
        /// </summary>
        public string Ref { get; set; }

        /// <summary>
        /// Gets or sets the proposal status
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// Gets or sets the proposal start date
        /// </summary>
        [IgnoreDataMemberAttribute]
        public DateTime? ProposalStartDate { get; set; }

        /// <summary>
        /// Gets or sets the proposal end date
        /// </summary>
        [IgnoreDataMemberAttribute]
        public DateTime? ProposalEndDate { get; set; }

        /// <summary>
        /// Gets or sets proposal created date
        /// </summary>
        [IgnoreDataMemberAttribute]
        public DateTime? CreatedDate { get; set; }

        /// <summary>
        /// Gets or sets the proposal submittal date
        /// </summary>
        [IgnoreDataMemberAttribute]
        public DateTime? SubmittalDate { get; set; }

        /// <summary>
        /// Gets or sets the proposal start date
        /// </summary>
        public string StDate { get; set; }

        /// <summary>
        /// Gets or sets the proposal end date
        /// </summary>
        public string EndDate { get; set; }

        /// <summary>
        /// Gets or sets proposal created date
        /// </summary>
        public string CrDate { get; set; }

        /// <summary>
        /// Gets or sets the proposal submittal date
        /// </summary>
        public string SubDate { get; set; }

        /// <summary>
        /// Gets or sets the elements of cost
        /// </summary>
        public string EoC { get; set; }

        /// <summary>
        /// Gets or sets the isIWTA bool
        /// </summary>
        public bool IWTA { get; set; }

        /// <summary>
        /// Gets or sets Prime Or Sub
        /// </summary>
        public string PrmSub { get; set; }

        /// <summary>
        /// Gets or sets the program name
        /// </summary>
        public string PrgName { get; set; }

        /// <summary>
        /// Gets or sets the contract leader/PoC
        /// </summary>
        public string ContLdr { get; set; }

        /// <summary>
        /// Gets or sets the RFP/Contract Modification Number
        /// </summary>
        public string RFP { get; set; }

        /// <summary>
        /// Gets or sets the Capture Manager
        /// </summary>
        public string Mgr { get; set; }

        /// <summary>
        /// Gets or sets the indpependent reviewer.
        /// </summary>
        public string IndpRev { get; set; }

        /// <summary>
        /// Gets or sets the Cover Sheet Approver.
        /// </summary>
        public string CSA { get; set; }

        /// <summary>
        /// Gets or sets the Pricing Verifier.
        /// </summary>
        public string PV { get; set; }

        /// <summary>
        /// Gets or sets the Material Lead.
        /// </summary>
        public string ML { get; set; }

        /// <summary>
        /// Gets or sets the Subcontract Lead.
        /// </summary>
        public string SL { get; set; }
    }
}