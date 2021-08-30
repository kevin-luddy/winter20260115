// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace RPM.DataBridge.Models
{
    /// <summary>
    /// The OTIS Opportunity Model.
    /// </summary>
    [Serializable]
    public class OTISOpportunityModelView
    {
        /// <summary>
        /// Gets or sets the OTIS Number.
        /// </summary>
        public string OTISNumber { get; set; }
        
        /// <summary>
        /// Gets or sets the Status.
        /// </summary>
        public string Status { get; set; }
       
        /// <summary>
        /// Gets or sets the Title.
        /// </summary>
        public string Title { get; set; }
        
        /// <summary>
        /// Gets or sets the Description.
        /// </summary>
        public string Description { get; set; }
        
        /// <summary>
        /// Gets or sets the Customer.
        /// </summary>
        public string Customer { get; set; }
        
        /// <summary>
        /// Gets or sets the LOB.
        /// </summary>
        public string LOB { get; set; } 
        
        /// <summary>
        /// Gets or sets the total program value.
        /// </summary>
        public double? TotalProgramValue { get; set; } 
        
        /// <summary>
        /// Gets or sets the sec code identifier.
        /// </summary>
        public string SecCodeId { get; set; } 
        
        /// <summary>
        /// Gets or sets the initial contract value.
        /// </summary>
        public string InitialContractValue { get; set; }
        
        /// <summary>
        /// Gets or sets the three yr orders value.
        /// </summary>
        public string ThreeYrOrders { get; set; } 
        
        /// <summary>
        /// Gets or sets the plan orders, a single char from financial line "B" "A" "C" "D" "E" may need on filter as well.
        /// </summary>
        public string PlanOrders { get; set; }
        
        /// <summary>
        /// Gets or sets a value indicating whether this Opportunity is international or domestic.
        /// </summary>
        public bool International { get; set;  }
        
        /// <summary>
        /// Gets or sets the Program Area.
        /// </summary>
        public string PA { get; set; }
        
        /// <summary>
        /// Gets or sets the category.
        /// </summary>
        public string Category { get; set; }
        
        /// <summary>
        /// Gets or sets the type of the opp.
        /// </summary>
        public string OppType { get; set; }
        
        /// <summary>
        /// Gets or sets the type of the contract.
        /// </summary>
        public string ContractType { get; set; }
        
        /// <summary>
        /// Gets or sets the role.
        /// </summary>
        public string Role { get; set; }
        
        /// <summary>
        /// Gets or sets the competitive value.
        /// </summary>
        public string Competitive { get; set; }
        
        /// <summary>
        /// Gets or sets the classification (Classified, Unclassified, DOD, etc).
        /// </summary>
        public string Classified { get; set; }
        
        /// <summary>
        /// Gets or sets the location.
        /// </summary>
        public string Location { get; set; }
        
        #region Milestone        
        /// <summary>
        /// Gets or sets the milestone name "Final RFP" date.
        /// </summary>
        public DateTime? RFPDate { get; set; }         
        
        /// <summary>
        /// Gets or sets the milestone name "Award" date.
        /// </summary>
        public DateTime? AwardDate { get; set; } 
        
        /// <summary>
        /// Gets or sets the milestone date for name: Prop Delivery (Planned).
        /// </summary>
        public DateTime? PlannedDate { get; set; } 
        
        /// <summary>
        /// Gets or sets the milestone date for name: Prop Delivery (Actual).
        /// </summary>
        public DateTime? ActualDate { get; set; }  
        
        /// <summary>
        /// Gets or sets the milestone name "Final RFP" date.
        /// </summary>
        public string RFPDateString { get; set; } 
        
        /// <summary>
        /// Gets or sets the end date for RFP.
        /// Check mapping, need valid start/end dates for Gantt.
        /// </summary>
        public string RFPEndDateString { get; set; } 
        
        /// <summary>
        /// Gets or sets the milestone name "Award" date.
        /// </summary>
        public string AwardDateString { get; set; } 
        
        /// <summary>
        /// Gets or sets the milestone date for name: Prop Delivery (Planned).
        /// </summary>
        public string PlannedDateString { get; set; } 
        
        /// <summary>
        /// Gets or sets the milestone date for name: Prop Delivery (Actual).
        /// </summary>
        public string ActualDateString { get; set; }
        #endregion Milestone

        #region Staff        
        /// <summary>
        /// Gets or sets the BD Manager.
        /// </summary>
        public string BDMgr { get; set; }
        
        /// <summary>
        /// Gets or sets the Capture Manager.
        /// </summary>
        public string CaptMgr { get; set; }
        
        /// <summary>
        /// Gets or sets the Proposal Manager.
        /// </summary>
        public string PM { get; set; }
        
        /// <summary>
        /// Gets or sets the Pricing\/CE Lead.
        /// </summary>
        public string CELead { get; set; } // staff name ", 
        
        /// <summary>
        /// Gets or sets the BD Lead.
        /// </summary>
        public string BDLead { get; set; }
        #endregion Staff

        /// <summary>
        /// Gets or sets the estimated value in millions
        /// </summary>
        public decimal? EVal { get; set; }
    }

    #region CLASSES DERIVED FROM JSON WEB SERVICE 

    /// <summary>
    /// An external teammate.
    /// </summary>
    [SuppressMessage("Microsoft.StyleCop.CSharp.MaintainabilityRules", "SA1402:FileMayOnlyContainASingleClass")]
    public class ExternalTeammate
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public string Name { get; set; }
    }

    /// <summary>
    /// An internal teammate.
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "*")]
    public class InternalTeammate
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public string Name { get; set; }
    }

    /// <summary>
    /// Teammates broken out into internal and external.
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "*")]
    public class Teammates
    {
        /// <summary>
        /// Gets or sets the external teammates.
        /// </summary>
        public ICollection<ExternalTeammate> ExternalTeammates { get; set; }
        
        /// <summary>
        /// Gets or sets the internal teammates.
        /// </summary>
        public ICollection<InternalTeammate> InternalTeammates { get; set; }
    }

    /// <summary>
    /// A Milestone for a program.
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "*")]
    public class Milestone
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// Gets or sets the date.
        /// </summary>
        public string Date { get; set; }
        
        /// <summary>
        /// Gets or sets the UTC date.
        /// </summary>
        public object UtcDate { get; set; }
        
        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="Milestone"/> is completed.
        /// </summary>
        public bool Completed { get; set; }
        
        /// <summary>
        /// Gets or sets the time.
        /// </summary>
        public string Time { get; set; }
    }

    /// <summary>
    /// Milestone dates in a program.
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "*")]
    public class Milestones
    {
        /// <summary>
        /// Gets or sets the milestone.
        /// </summary>
        public ICollection<Milestone> Milestone { get; set; }
    }

    /// <summary>
    /// A point of contact.
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "*")]
    public class Poc
    {
        /// <summary>
        /// Gets or sets the role.
        /// </summary>
        public string Role { get; set; }
        
        /// <summary>
        /// Gets or sets the ntid.
        /// </summary>
        public string Ntid { get; set; }
       
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// Gets or sets the phone.
        /// </summary>
        public string Phone { get; set; }
        
        /// <summary>
        /// Gets or sets the email.
        /// </summary>
        public string Email { get; set; }
    }

    /// <summary>
    /// Staff Points of Contact.
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "*")]
    public class Staff
    {
        /// <summary>
        /// Gets or sets the pocs.
        /// </summary>
        public ICollection<Poc> Pocs { get; set; }
    }

    /// <summary>
    /// A Subcontractor Vendor.
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "*")]
    public class SubsVendor
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public string Name { get; set; }
    }

    /// <summary>
    /// A Competitor in OTIS
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "*")]
    public class Competitor
    {
        /// <summary>
        /// Gets or sets the prime.
        /// </summary>
        public string Prime { get; set; }
        
        /// <summary>
        /// Gets or sets the strategy.
        /// </summary>
        public string Strategy { get; set; }
        
        /// <summary>
        /// Gets or sets the subs vendors.
        /// </summary>
        public ICollection<SubsVendor> SubsVendors { get; set; }
    }

    /// <summary>
    /// A list of Competitors.
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "*")]
    public class Competitors
    {
        /// <summary>
        /// Gets or sets the competitor.
        /// </summary>
        public ICollection<Competitor> Competitor { get; set; }
    }

    /// <summary>
    /// A financial row in OTIS.
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "*")]
    public class FinancialRow
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// Gets or sets the type of the fr.
        /// </summary>
        public string FRType { get; set; }
        
        /// <summary>
        /// Gets or sets the prior yrs.
        /// </summary>
        public string PriorYrs { get; set; }
        
        /// <summary>
        /// Gets or sets the yr0.
        /// </summary>
        public string Yr0 { get; set; }
        
        /// <summary>
        /// Gets or sets the yr1.
        /// </summary>
        public string Yr1 { get; set; }
        
        /// <summary>
        /// Gets or sets the yr2.
        /// </summary>
        public string Yr2 { get; set; }
        
        /// <summary>
        /// Gets or sets the yr3.
        /// </summary>
        public string Yr3 { get; set; }
        
        /// <summary>
        /// Gets or sets the yr4.
        /// </summary>
        public string Yr4 { get; set; }
        
        /// <summary>
        /// Gets or sets the yr5.
        /// </summary>
        public string Yr5 { get; set; }
        
        /// <summary>
        /// Gets or sets the future yrs.
        /// </summary>
        public string FutureYrs { get; set; }
        
        /// <summary>
        /// Gets or sets the total.
        /// </summary>
        public string Total { get; set; }
    }

    /// <summary>
    ///  A financial summary in OTIS.
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "*")]
    public class FinancialSummary
    {
        /// <summary>
        /// Gets or sets the financial rows.
        /// </summary>
        public ICollection<FinancialRow> FinancialRows { get; set; }
    }

    /// <summary>
    /// A collection of Opportunities.
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "*")]
    public class OppCollection
    {
        /// <summary>
        /// Gets or sets the opportunity identifier.
        /// </summary>
        public string OpportunityId { get; set; }
        
        /// <summary>
        /// Gets or sets the name of the opportunity.
        /// </summary>
        public string OpportunityName { get; set; }
        
        /// <summary>
        /// Gets or sets the sec code identifier.
        /// </summary>
        public string SecCodeId { get; set; }
        
        /// <summary>
        /// Gets or sets the acronym.
        /// </summary>
        public string Acronym { get; set; }
        
        /// <summary>
        /// Gets or sets the lob.
        /// </summary>
        public string LOB { get; set; }
        
        /// <summary>
        /// Gets or sets the status.
        /// </summary>
        public string Status { get; set; }
        
        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        public string Description { get; set; }
        
        /// <summary>
        /// Gets or sets the customer.
        /// </summary>
        public string Customer { get; set; }
        
        /// <summary>
        /// Gets or sets the win strategy.
        /// </summary>
        public string WinStrategy { get; set; }
        
        /// <summary>
        /// Gets or sets the issues concerns.
        /// </summary>
        public string IssuesConcerns { get; set; }
        
        /// <summary>
        /// Gets or sets the initial contract value.
        /// </summary>
        public string InitialContractValue { get; set; }
        
        /// <summary>
        /// Gets or sets the three yr orders.
        /// </summary>
        public string ThreeYrOrders { get; set; }
        
        /// <summary>
        /// Gets or sets the isgs value.
        /// </summary>
        public double ISGSValue { get; set; }
        
        /// <summary>
        /// Gets or sets the pgo.
        /// </summary>
        public string Pgo { get; set; }
        
        /// <summary>
        /// Gets or sets the pw.
        /// </summary>
        public string Pw { get; set; }
        
        /// <summary>
        /// Gets or sets the type of the opp.
        /// </summary>
        public string OppType { get; set; }
        
        /// <summary>
        /// Gets or sets the type of the contract.
        /// </summary>
        public string ContractType { get; set; }
        
        /// <summary>
        /// Gets or sets the domestic overseas.
        /// </summary>
        public string DomesticOverseas { get; set; }
        
        /// <summary>
        /// Gets or sets the category.
        /// </summary>
        public string Category { get; set; }
        
        /// <summary>
        /// Gets or sets the competitive.
        /// </summary>
        public string Competitive { get; set; }
        
        /// <summary>
        /// Gets or sets the bid role.
        /// </summary>
        public string BidRole { get; set; }
        
        /// <summary>
        /// Gets or sets the property location city.
        /// </summary>
        public string PropLocationCity { get; set; }
        
        /// <summary>
        /// Gets or sets the state of the property location.
        /// </summary>
        public string PropLocationState { get; set; }
        
        /// <summary>
        /// Gets or sets the property location country.
        /// </summary>
        public string PropLocationCountry { get; set; }
        
        /// <summary>
        /// Gets or sets the classified.
        /// </summary>
        public string Classified { get; set; }
        
        /// <summary>
        /// Gets or sets the teammates.
        /// </summary>
        public Teammates Teammates { get; set; }
        
        /// <summary>
        /// Gets or sets the milestones.
        /// </summary>
        public Milestones Milestones { get; set; }
        
        /// <summary>
        /// Gets or sets the staff.
        /// </summary>
        public Staff Staff { get; set; }
        
        /// <summary>
        /// Gets or sets the competitors.
        /// </summary>
        public Competitors Competitors { get; set; }
        
        /// <summary>
        /// Gets or sets the financial summary.
        /// </summary>
        public FinancialSummary FinancialSummary { get; set; }
        
        /// <summary>
        /// Gets or sets the total program value.
        /// </summary>
        public double? TotalProgramValue { get; set; }
    }

    /// <summary>
    /// The Root object in XML.
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "*")]
    public class RootObject
    {
        /// <summary>
        /// Gets or sets the opp collection.
        /// </summary>
        public ICollection<OppCollection> OppCollection { get; set; }

        /// <summary>
        /// Gets or sets the name of the collection.
        /// </summary>
        /// <value>
        /// The name of the collection.
        /// </value>
        public object CollectionName { get; set; }
    }

    #endregion CLASSES DERIVED FROM JSON WEB SERVICE 
}
