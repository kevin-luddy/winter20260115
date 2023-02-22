/*
    Copyright 2016-2020 Lockheed Martin Corporation.

    This computer software has been provided in confidence, and contains trade secret and/or privileged or confidential 
    commercial or financial information. Public disclosure of any information marked as indicated above is prohibited 
    by the Trade Secrets Act (18 U.S.C. Sec. 1905) and the Economic Espionage Act of 1996 (18 U.S.C. Sec. 1831 et seq.) 
    and is not to be made available to third parties without the prior written permission of Lockheed Martin Corporation.
*/

namespace APTSPropricerApi.DTOs
{
	using System.Collections.Generic;

	/// <summary>
	/// Data Transfer Object for General Proposal Data
	/// </summary>
	public class ProposalDto
	{
		//General proposal screen fields 
		/// <summary>
		/// Gets or Sets the Id
		/// </summary>
		public string Id { get; set; }

		/// <summary>
		/// Gets or Sets the Name
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Gets or Sets the Version
		/// </summary>
		public string Version { get; set; }

		/// <summary>
		/// Gets or Sets the Creator Name
		/// </summary>
		public string CreatorName { get; set; }

		/// <summary>
		/// Gets or Sets the Number
		/// </summary>
		public string Number { get; set; }

		/// <summary>
		/// Gets or Sets the Description
		/// </summary>
		public string Description { get; set; }

		/// <summary>
		/// Gets or Sets the Title
		/// </summary>
		public string Title { get; set; }

		/// <summary>
		/// Gets or Sets the Parent Folder Name
		/// </summary>
		public string ParentFolderName { get; set; }

		/// <summary>
		/// Gets or Sets the Manager
		/// </summary>
		public string Manager { get; set; }

		/// <summary>
		/// Gets or Sets the Business Unit
		/// </summary>
		public string BusinessUnit { get; set; }

		/// <summary>
		/// Gets or Sets the Rfq
		/// </summary>
		public string Rfq { get; set; }

		/// <summary>
		/// Gets or Sets the Start Date TimeFrame in ProPricer
		/// </summary>
		public string StartDate { get; set; }

		/// <summary>
		/// Gets or Sets the End Date TimeFrame in ProPricer
		/// </summary>
		public string EndDate { get; set; }

		/// <summary>
		/// Gets or Sets the Number of Months
		/// </summary>
		public int NumberMonths { get; set; }

		/// <summary>
		/// Gets or Sets the Due Date TimeFrame in ProPricer
		/// </summary>
		public string DueDate { get; set; }

		/// <summary>
		/// Gets or Sets the Resource Decimal Precision
		/// </summary>
		public byte? ResourceDecimalPercision { get; set; }

		/// <summary>
		/// Gets or Sets the Award Probability
		/// </summary>
		public string AwardProbability { get; set; }

		/// <summary>
		/// Gets or Sets the Target Price
		/// </summary>
		public string TargetPrice { get; set; }

		/// <summary>
		/// Gets or Sets the Global Profit Factor
		/// </summary>
		public string GlobalProfitFactor { get; set; }

		//Pricing screens fields

		/// <summary>
		/// Gets or Sets the Direct Rate Table
		/// </summary>
		public string DirectRateTable { get; set; }

		/// <summary>
		/// Gets or Sets the Burden Rate Table
		/// </summary>
		public string BurdenRateTable { get; set; }

		/// <summary>
		/// Gets or Sets the Travel Rate Table
		/// </summary>
		public string TravelRateTable { get; set; }

		/// <summary>
		/// Gets or Sets the Factor Rate Table
		/// </summary>
		public string FactorRateTable { get; set; }

		//Summary fields screen

		/// <summary>
		/// Gets or Sets the Task Id Label
		/// </summary>
		public string TaskIdLabel { get; set; }

		/// <summary>
		/// Gets or Sets the Summary Field Definitions
		/// </summary>
		public IEnumerable<SummaryFieldDefinitionsDto> SumFieldDefs { get; set; }

		//Notes screen 

		/// <summary>
		/// Gets or Sets the Notes
		/// </summary>
		public string Notes { get; set; }

		//Calendar screen  

		/// <summary>
		/// Gets or Sets the Fiscal Year Start Month Offset
		/// </summary>
		public int FiscalYearStartMonthOffset { get; set; }

		//Reports – footer screen

		/// <summary>
		/// Gets or Sets the Report Footer
		/// </summary>
		public string RptFooter { get; set; }

		/// <summary>
		/// Gets or Sets the Array of tasks – only used to collect all tasks (for TaskDto breakout see Task Controller)
		/// </summary>
		public IEnumerable<TaskDto> Tasks { get; set; }
	}
}